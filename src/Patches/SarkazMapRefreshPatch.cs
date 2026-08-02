using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Model;

namespace Skz.Ocean.Patches;

// 节点刷新：在每个普通节点正右侧放一个刷新按钮。
// 花 1 构想把该节点刷成不同的随机类型（仅可达节点、有剩余刷新次数、可被修改的节点）。
// 节点类型改完由官方地图存档自动持久化；刷新次数由 SarkazSaveIO 单独存。
internal static class SarkazMapRefresh
{
    // 不能给官方类加字段，用弱表把按钮挂到节点上。
    public static readonly ConditionalWeakTable<NNormalMapPoint, TextureButton> Buttons = new();

    private const float _rightOffset = 65f; // 节点局部坐标里向右的偏移（“正右”）

    private static Traverse _trav = null;

    public static TextureButton CreateButton()
    {
        Texture2D tex = ResourceLoader.Load<Texture2D>(ImageHelper.GetImagePath("ui/refresh.png"));
        return new TextureButton
        {
            TextureNormal = tex,
            IgnoreTextureSize = true,
            StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered,
            CustomMinimumSize = new Vector2(40, 40),
            Size = new Vector2(40, 40),
            MouseFilter = Control.MouseFilterEnum.Stop,
            Visible = false,
        };
    }

    // 把按钮位置同步到 NMapPoint 正右侧（按钮已挂在父容器上，和节点平级）
    private static void SyncButtonPosition(NNormalMapPoint node, TextureButton btn)
    {
        btn.GlobalPosition = node.GlobalPosition + new Vector2(_rightOffset, -20f);
    }

    public static void UpdateButton(NNormalMapPoint node, SarkazManagerModel mgr)
    {
        if (!Buttons.TryGetValue(node, out TextureButton btn) || !GodotObject.IsInstanceValid(btn)) return;
        btn.Visible = false;
        if (!mgr.CanRefreshNode(node.Point)) return;

        //get nodes connected to the current node
        //vertical
        if (_trav == null) _trav = Traverse.Create(NMapScreen.Instance);
        RunState runState = _trav.Field("_runState").GetValue<RunState>();
        var dict = _trav.Field("_mapPointDictionary").GetValue<Dictionary<MapCoord, NMapPoint>>();
        ActMap map = _trav.Field("_map").GetValue<ActMap>();
        
        if (runState == null || dict == null || !runState.VisitedMapCoords.Any()) return;
        IReadOnlyList<MapCoord> visited = runState.VisitedMapCoords;
        if (visited.Contains(node.Point.coord)) return;
        MapCoord current = visited[visited.Count - 1];
        if (dict.TryGetValue(current, out NMapPoint mapPoint))
        {
            btn.Visible = MapTravel.GetTravelablePointsFrom(runState, mapPoint.Point).Contains(node.Point);
            if (btn.Visible) {
                SyncButtonPosition(node, btn); return;
            }
        }
        Log.Info("[sarkaz] check horizontal");

        //horizontal
        if (current.row >= map.GetRowCount() - 1 || node.Point.coord.row != current.row) return;
        List<MapPoint> rowPoints = map.GetPointsInRow(current.row).ToList();
        int idx = rowPoints.FindIndex(p => p.coord == current);
        int idx2 = rowPoints.FindIndex(p => p.coord == node.Point.coord);
        btn.Visible = Math.Abs(idx - idx2) == 1;
        if (btn.Visible) SyncButtonPosition(node, btn);
    }

    public static void OnPressed(NNormalMapPoint node, SarkazManagerModel mgr)
    {
        // if (node.State != MapPointState.Travelable) return;
        if (!mgr.RefreshNode(node.Point)) return;  
        // UpdateIcon 是 private —— 改完 PointType 后重绘图标。
        Traverse.Create(node).Method("UpdateIcon").GetValue();
        // 本节点刷新次数变了，单独刷一下；构想变化由 PlayerThoughtChanged 覆盖所有按钮。
        UpdateButton(node, mgr);
        // 刷新发生在地图界面，官方只在房间切换时存档——主动存一次，让新类型+刷新次数立即落盘。
        _ = SaveManager.Instance.SaveRun(null);
    }

    public static SarkazManagerModel GetManager(NNormalMapPoint node)
    {
        IRunState runState = Traverse.Create(node).Field("_runState").GetValue<IRunState>();
        return LocalContext.GetMe(runState)?.GetSarkazManager();
    }
}

[HarmonyPatch(typeof(NNormalMapPoint), "_Ready")]
public static class SarkazMapPointReadyPatch
{
    public static void Postfix(NNormalMapPoint __instance)
    {
        try
        {
            IRunState runState = Traverse.Create(__instance).Field("_runState").GetValue<IRunState>();
            Player me = LocalContext.GetMe(runState);
            SarkazManagerModel mgr = me?.GetSarkazManager();
            if (mgr == null) return;

            TextureButton btn = SarkazMapRefresh.CreateButton();
            SarkazMapRefresh.Buttons.AddOrUpdate(__instance, btn);
            // 延迟到下一帧再挂到父容器上（确保 GetParent 不为 null），
            // 平级挂载让按钮独立于 NMapPoint 的 Enable/Disable 状态。
            Callable.From(() =>
            {
                Node parent = __instance.GetParent();
                if (parent != null)
                {
                    parent.AddChild(btn);
                    SarkazMapRefresh.UpdateButton(__instance, mgr);
                }
            }).CallDeferred();

            btn.Pressed += () => SarkazMapRefresh.OnPressed(__instance, mgr);

            // 构想数量变化时（刷新扣构想、或别处增减）所有按钮重新评估可见性。
            PlayerExtraData extra = me.GetExtraData();
            void Handler() => SarkazMapRefresh.UpdateButton(__instance, mgr);
            extra.PlayerThoughtChanged += Handler;
            btn.TreeExiting += () => extra.PlayerThoughtChanged -= Handler;
        }
        catch (Exception e)
        {
            Log.Error($"[Sarkaz] 刷新按钮初始化失败: {e}");
        }
    }
}

[HarmonyPatch(typeof(NNormalMapPoint), "RefreshState")]
public static class SarkazMapPointRefreshStatePatch
{
    public static void Postfix(NNormalMapPoint __instance)
    {
        if (!SarkazMapRefresh.Buttons.TryGetValue(__instance, out _)) return;
        SarkazManagerModel mgr = SarkazMapRefresh.GetManager(__instance);
        if (mgr == null) return;
        SarkazMapRefresh.UpdateButton(__instance, mgr);
    }
}
