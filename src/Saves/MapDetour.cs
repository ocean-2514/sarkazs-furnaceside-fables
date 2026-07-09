using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using Skz.Ocean.Model.Acts;

namespace Skz.Ocean.Saves;

// ──────────────────────────────────────────────────────────────
//  存档结构：一摞"返回书签"。用栈支持子地图里再进子地图。
// ──────────────────────────────────────────────────────────────
public sealed class MapDetourSave
{
    [JsonPropertyName("schema_version")] public int SchemaVersion { get; set; } = 1;

    [JsonPropertyName("frames")] public List<DetourFrame> Frames { get; set; } = new();
}

public sealed class DetourFrame
{
    // 回到哪一层
    [JsonPropertyName("return_act_index")] public int ReturnActIndex { get; set; }

    // 回去时的层内楼层数
    [JsonPropertyName("return_act_floor")] public int ReturnActFloor { get; set; }

    // 原层走过的完整路径（最后一个 = 触发事件的那个节点）
    [JsonPropertyName("visited_coords")] public List<MapCoord> VisitedCoords { get; set; } = new();

    // 原层的地图快照。引擎存档只保存"当前层"地图，离开后原图会丢，所以必须自己带。
    [JsonPropertyName("return_map")] public SerializableActMap ReturnMap { get; set; } = null!;
}

// ──────────────────────────────────────────────────────────────
//  进入 / 返回诡谲断章(子地图)的存储与还原逻辑。
//  文件落在 profileN/saves/sarkaz_detour_{startTime}.save，
//  用 run 的 StartTime 命名以绑定到当前这一局，避免串档。
//
//  子层 act 不进 GetDefaultList，而是进入时动态 append 到 RunState.Acts、
//  返回时移除——正常流程 Acts.Count-1 始终指向真实最终层，原版所有
//  终局判定(EnterNextAct 胜利分支/最终 Boss 无奖励/WinTime)不受影响。
// ──────────────────────────────────────────────────────────────
public static class MapDetour
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        IncludeFields = true, // MapCoord 用 [JsonInclude] 的字段
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    // RunManager._startTime 是私有字段，用 Harmony 的 Traverse 取，和原版 run 存档同款命名。
    private static long RunStartTime()
        => Traverse.Create(RunManager.Instance).Field("_startTime").GetValue<long>();

    // 根目录指向账号目录，传入的是 profile 相对路径——和引擎自己的 SaveStore 完全一致。
    private static GodotFileIo Io()
        => new GodotFileIo(UserDataPathProvider.GetAccountScopedBasePath(null));

    private static string FilePath(long startTime)
        => Path.Combine(
            UserDataPathProvider.GetProfileDir(SaveManager.Instance.CurrentProfileId),
            UserDataPathProvider.SavesDir,
            $"sarkaz_detour_{startTime}.save");

    private static MapDetourSave Load(long startTime)
    {
        try
        {
            string json = Io().ReadFile(FilePath(startTime));
            if (string.IsNullOrWhiteSpace(json))
            {
                return new MapDetourSave();
            }
            return JsonSerializer.Deserialize<MapDetourSave>(json, JsonOpts) ?? new MapDetourSave();
        }
        catch (Exception e)
        {
            Log.Error($"[Sarkaz] Failed to load detour save: {e}");
            return new MapDetourSave();
        }
    }

    private static void Store(MapDetourSave data, long startTime)
    {
        try
        {
            GodotFileIo io = Io();
            string path = FilePath(startTime);
            if (data.Frames.Count == 0)
            {
                // 栈空了就清掉文件，避免残留串到下一局
                if (io.FileExists(path)) io.DeleteFile(path);
                if (io.FileExists(path + ".backup")) io.DeleteFile(path + ".backup");
                return;
            }
            io.WriteFile(path, JsonSerializer.Serialize(data, JsonOpts));
        }
        catch (Exception e)
        {
            Log.Error($"[Sarkaz] Failed to write detour save: {e}");
        }
    }

    // 是否正在支线层里（有未弹出的返回书签）。
    public static bool IsInDetour()
    {
        return Load(RunStartTime()).Frames.Count > 0;
    }

    // ── 子层美术/音频资源沿用"来源 act" ──
    // 进入时直接缓存；读档续玩时缓存为空，从书签的 ReturnActIndex 懒加载。
    private static ActModel _sourceActCache;

    public static ActModel GetSourceAct()
    {
        if (_sourceActCache != null) return _sourceActCache;
        try
        {
            RunState state = Traverse.Create(RunManager.Instance).Property("State").GetValue<RunState>();
            if (state == null) return null;
            MapDetourSave data = Load(RunStartTime());
            // 从最深的书签往下找第一个"真实层"（嵌套子层时跳过 BizarreFragment）
            for (int i = data.Frames.Count - 1; i >= 0; i--)
            {
                int idx = data.Frames[i].ReturnActIndex;
                if (idx >= 0 && idx < state.Acts.Count && state.Acts[idx] is not BizarreFragment)
                {
                    _sourceActCache = state.Acts[idx];
                    return _sourceActCache;
                }
            }
        }
        catch (Exception e)
        {
            Log.Warn($"[Sarkaz] GetSourceAct failed: {e.Message}");
        }
        return null;
    }

    // FilePathIdentifier 补丁用（背景场景/地图贴图/篝火场景路径）。
    public static string GetSourceActIdentifier()
    {
        return GetSourceAct()?.Id.Entry.ToLowerInvariant();
    }

    // 换局读档时清缓存，避免上一局的来源 act 残留（SarkazPlayerLoadPatch 调用）。
    public static void ClearSourceActCache()
    {
        _sourceActCache = null;
    }

    // ── 进入诡谲断章：压返回书签 → 动态追加子层 act → EnterAct。──
    public static async Task EnterFragment(RunState state)
    {
        if (state == null)
        {
            Log.Warn("[Sarkaz] EnterFragment: run state is null");
            return;
        }
        // 记录来源 act（嵌套进入时保留最外层的真实层）
        if (state.Act is not BizarreFragment)
        {
            _sourceActCache = state.Act;
        }

        PushCurrentLocation(state);

        // 动态追加子层 act（不进 GetDefaultList，避免影响原版终局判定）
        ActModel fragment = ModelDb.Act<BizarreFragment>().ToMutable();
        // 开局的 RunManager.GenerateRooms 只遍历当时的 Acts 列表，动态追加的层
        // 必须自己补生成房间集，否则 RoomSet.Boss 未设置 → LoadActAssets 崩溃。
        fragment.GenerateRooms(state.Rng.UpFront, state.UnlockState, state.Players.Count > 1);

        List<ActModel> acts = state.Acts.ToList();
        acts.Add(fragment);
        Traverse.Create(state).Property("Acts").SetValue(acts);

        await RunManager.Instance.EnterAct(acts.Count - 1);
    }

    // ── 进入子地图前调用：把"我现在在哪"压栈存盘。必须在 EnterAct 之前调用！──
    public static void PushCurrentLocation(RunState state)
    {
        if (state == null)
        {
            Log.Warn("[Sarkaz] PushCurrentLocation: run state is null");
            return;
        }
        if (state.Map == null)
        {
            Log.Warn("[Sarkaz] PushCurrentLocation: current map is null");
            return;
        }

        long startTime = RunStartTime();
        MapDetourSave data = Load(startTime);
        data.Frames.Add(new DetourFrame
        {
            ReturnActIndex = state.CurrentActIndex,
            ReturnActFloor = state.ActFloor,
            VisitedCoords = state.VisitedMapCoords.ToList(),
            ReturnMap = SerializableActMap.FromActMap(state.Map),
        });
        Store(data, startTime);
        Log.Info($"[Sarkaz] Pushed return bookmark: act={state.CurrentActIndex}, " +
                 $"floor={state.ActFloor}, visited={state.VisitedMapCoords.Count}");
    }

    // ── 在子地图里调用：弹栈，回到原层、原地图、原节点，可继续选路。──
    public static async Task PopAndReturn(RunState state)
    {
        RunManager rm = RunManager.Instance;
        if (state == null)
        {
            Log.Warn("[Sarkaz] PopAndReturn: run state is null");
            return;
        }

        long startTime = RunStartTime();
        MapDetourSave data = Load(startTime);
        if (data.Frames.Count == 0)
        {
            Log.Warn("[Sarkaz] PopAndReturn: no bookmark to return to");
            return;
        }

        DetourFrame frame = data.Frames[^1];
        data.Frames.RemoveAt(data.Frames.Count - 1);

        if (frame.ReturnActIndex < 0 || frame.ReturnActIndex >= state.Acts.Count || frame.ReturnMap == null)
        {
            Log.Error($"[Sarkaz] PopAndReturn: invalid bookmark (act={frame.ReturnActIndex}), aborting");
            Store(data, startTime); // 仍然把损坏帧弹出去
            return;
        }

        // 让 GenerateMap 用快照精确重建原层地图，而不是随机生成
        rm.SavedMapsToLoad ??= new Dictionary<int, SerializableActMap>();
        rm.SavedMapsToLoad[frame.ReturnActIndex] = frame.ReturnMap;

        // EnterAct 对 act 0 + Neow 开局有特例：会直接 EnterMapCoord(起点) 把人扔回起点房间。
        // 我们要"回到半路"，所以返回 act 0 时临时关掉 StartedWithNeow，逼它走普通 MapRoom 分支。
        // 因为地图是用快照重建的（走 SavedMapsToLoad 分支），这个标志不会影响地图生成。
        bool neowToggled = false;
        bool prevNeow = state.ExtraFields.StartedWithNeow;
        if (frame.ReturnActIndex == 0 && prevNeow)
        {
            state.ExtraFields.StartedWithNeow = false;
            neowToggled = true;
        }
        try
        {
            // EnterAct 会：淡出、清屏、退房间、切层(清空 visited)、重建地图、进入地图选择界面
            await rm.EnterAct(frame.ReturnActIndex);
        }
        finally
        {
            if (neowToggled)
            {
                state.ExtraFields.StartedWithNeow = prevNeow;
            }
        }

        // 已经回到原层：把动态追加的子层 act 从列表尾部摘掉，恢复原版 Acts.Count。
        List<ActModel> acts = state.Acts.ToList();
        while (acts.Count > 0 && acts[^1] is BizarreFragment)
        {
            acts.RemoveAt(acts.Count - 1);
        }
        Traverse.Create(state).Property("Acts").SetValue(acts);

        // EnterAct 清掉了走过的路径，这里重放回去，让玩家站在事件节点上
        foreach (MapCoord coord in frame.VisitedCoords)
        {
            state.AddVisitedMapCoord(coord);
        }
        state.ActFloor = frame.ReturnActFloor;

        // 关键：EnterAct 内部的 GenerateMap 是在 visited 为空时跑的，所以地图 UI 按"起点"渲染了
        // （走过的路径只在 SetMap 里上色，节点可走/已走状态只在 RecalculateTravelability 里算，
        //   SetTravelEnabled 只是重绘、不会重算）。这里在 visited 还原后再重建一次地图 UI。
        rm.SavedMapsToLoad ??= new Dictionary<int, SerializableActMap>();
        rm.SavedMapsToLoad[frame.ReturnActIndex] = frame.ReturnMap; // EnterAct 的 GenerateMap 消费掉了，放回去
        // 对齐投票同步器到当前节点（否则 "accepting votes for coord (3,0)" 不匹配 → 没法选路）
        rm.MapSelectionSynchronizer.OnLocationChanged(state.MapLocation);
        rm.RunLocationTargetedBuffer.OnLocationChanged(state.RunLocation);
        await rm.GenerateMap();

        // 刷新界面：兜底重算 travelability（GenerateMap 仅在屏幕可见时才算）、移动标记
        NMapScreen screen = NMapScreen.Instance;
        MapCoord? current = state.CurrentMapCoord;
        if (screen != null && current.HasValue)
        {
            Traverse.Create(screen).Method("RecalculateTravelability").GetValue();
            screen.InitMarker(current.Value);
            screen.SetTravelEnabled(true);
            screen.RefreshAllMapPointVotes();
        }

        // 清掉来源 act 缓存（若仍在嵌套子层里，下次读取会从剩余书签懒加载）
        _sourceActCache = null;

        Store(data, startTime);
        Log.Info($"[Sarkaz] Returned to act={frame.ReturnActIndex}, " +
                 $"restored visited={frame.VisitedCoords.Count}, floor={frame.ReturnActFloor}");
    }
}
