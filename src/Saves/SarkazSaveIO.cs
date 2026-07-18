using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Model;

namespace Skz.Ocean.Saves;

// 萨卡兹专属数据的“侧存档”(side-file)。
//
// 为什么用侧存档而不是塞进游戏的 SerializablePlayer：后者是固定字段的 schema，
// 同时走 JSON 存档 + 二进制联机两条序列化路径，加字段要动 migration 且两条路都得改，风险高。
// 侧存档零 schema/migration 风险，仅单人。文件与官方存档放同一目录、同 profile 作用域：
//   <profile>/saves/current_run.sarkaz.save
// 写：RunManager.ToSave 时(SarkazSavePatch)。读：Player.FromSerializable 时按 NetId 匹配恢复。
//
// 用 Godot 的 Json(纯 Variant，无反射) 而非 System.Text.Json，避免导出版的 AOT/裁剪问题。
public static class SarkazSaveIO
{
    private const string _fileName = "saves/current_run.sarkaz.save";

    private static string FilePath => SaveManager.Instance.GetProfileScopedPath(_fileName);

    public static void Save(IRunState runState)
    {
        var players = new Godot.Collections.Array();
        foreach (Player player in runState.Players)
        {
            var thoughts = new Godot.Collections.Array();
            foreach (ThoughtModel t in player.GetAllThoughts())
            {
                thoughts.Add(ThoughtToDict(t));
            }

            var pdict = new Godot.Collections.Dictionary
            {
                { "net_id", player.NetId.ToString() }, // ulong 可能超出 double 精度，存成字符串
                { "shield", player.GetShield() },
                { "toil_limit", player.GetToilLimit() },
                { "thoughts", thoughts },
            };

            // “已使用思绪”不在三大列表里(使用时已移出)，单独存。用于战斗中途存档后读档仍能结算。
            ThoughtModel used = player.GetThoughtUsed();
            if (used != null)
            {
                pdict["thought_used"] = ThoughtToDict(used);
            }

            // 节点刷新次数（节点类型本身已由官方地图存档持久化，这里只存“刷了几次”）。
            SarkazManagerModel mgr = player.GetSarkazManager();
            if (mgr != null && mgr.RefreshCounts.Count > 0)
            {
                var refreshes = new Godot.Collections.Array();
                foreach (var kv in mgr.RefreshCounts)
                {
                    refreshes.Add(new Godot.Collections.Dictionary
                    {
                        { "col", kv.Key.col }, { "row", kv.Key.row }, { "count", kv.Value },
                    });
                }
                pdict["refresh_counts"] = refreshes;
            }

            // 旧乡晶尘等“待返还”思绪（进入新一层时发回手上）。
            if (mgr != null && mgr.PendingReturns.Count > 0)
            {
                var returns = new Godot.Collections.Array();
                foreach (ModelId id in mgr.PendingReturns) returns.Add(id.ToString());
                pdict["pending_returns"] = returns;
            }

            // 年代之刺击杀后、还没随战斗奖励发放的构想计数（战斗结束存档早于发奖）。
            if (mgr != null && mgr.PendingPlanRewards > 0)
            {
                pdict["pending_plan_rewards"] = mgr.PendingPlanRewards;
            }

            // 待消费的金币奖励加成（AfterCombatEnd 累加 → 存档 → 发奖消费，奖励界面 SL 需要它）。
            if (mgr != null && mgr.PendingGoldMultiplier > 0f)
            {
                pdict["pending_gold_multiplier"] = mgr.PendingGoldMultiplier;
            }

            // 诡谲断章行程标记："上一幕是否是子幕"（返程不结算换幕效果）。
            if (mgr != null && mgr.PreviousActWasFragment)
            {
                pdict["prev_act_was_fragment"] = true;
            }

            // 注定相遇：本局第一个 ? 房是否已触发。
            if (mgr != null && mgr.FatedEncounterDone)
            {
                pdict["fated_encounter_done"] = true;
            }

            // 诡谲断章固定事件节点坐标（按层）。地图重建(读档/回程)时靠它识别节点，不重掷。
            if (mgr != null && mgr.FragmentCoords.Count > 0)
            {
                var frags = new Godot.Collections.Array();
                foreach (var kv in mgr.FragmentCoords)
                {
                    foreach (MapCoord c in kv.Value)
                    {
                        frags.Add(new Godot.Collections.Dictionary
                        {
                            { "act", kv.Key }, { "col", c.col }, { "row", c.row },
                        });
                    }
                }
                pdict["fragment_coords"] = frags;
            }

            players.Add(pdict);
        }

        var root = new Godot.Collections.Dictionary { { "players", players } };
        string json = Json.Stringify(root);

        using FileAccess f = FileAccess.Open(FilePath, FileAccess.ModeFlags.Write);
        if (f == null)
        {
            Log.Warn($"[Sarkaz] 无法写侧存档 {FilePath}: {FileAccess.GetOpenError()}");
            return;
        }
        f.StoreString(json);
    }

    // 读侧存档，把与 player.NetId 匹配的那条记录恢复到该 player 上。返回是否找到。
    public static bool RestoreToPlayer(Player player)
    {
        Godot.Collections.Dictionary root = ReadRoot();
        if (root == null || !root.ContainsKey("players")) return false;

        foreach (Variant pv in root["players"].AsGodotArray())
        {
            Godot.Collections.Dictionary pd = pv.AsGodotDictionary();
            if (!pd.ContainsKey("net_id")) continue;
            if (!ulong.TryParse(pd["net_id"].AsString(), out ulong netId) || netId != player.NetId) continue;

            int shield = pd.ContainsKey("shield") ? pd["shield"].AsInt32() : 0;
            int toilLimit = pd.ContainsKey("toil_limit") ? pd["toil_limit"].AsInt32() : 3;
            player.GetExtraData().RestorePersistentState(shield, toilLimit);

            if (pd.ContainsKey("thoughts"))
            {
                foreach (Variant tv in pd["thoughts"].AsGodotArray())
                {
                    ThoughtModel thought = BuildThought(tv.AsGodotDictionary());
                    if (thought != null) player.AddThoughtInternal(thought);
                }
            }

            if (pd.ContainsKey("thought_used"))
            {
                ThoughtModel used = BuildThought(pd["thought_used"].AsGodotDictionary());
                if (used != null)
                {
                    used.Owner = player; // 不进列表，单独挂到“已使用思绪”
                    player.GetExtraData().RestoreThoughtUsed(used);
                }
            }

            if (pd.ContainsKey("refresh_counts"))
            {
                SarkazManagerModel mgr = player.GetSarkazManager();
                if (mgr != null)
                {
                    foreach (Variant rv in pd["refresh_counts"].AsGodotArray())
                    {
                        Godot.Collections.Dictionary rd = rv.AsGodotDictionary();
                        mgr.RestoreRefreshCount(new MapCoord(rd["col"].AsInt32(), rd["row"].AsInt32()), rd["count"].AsInt32());
                    }
                }
            }

            if (pd.ContainsKey("pending_returns"))
            {
                SarkazManagerModel mgr2 = player.GetSarkazManager();
                if (mgr2 != null)
                {
                    foreach (Variant rv in pd["pending_returns"].AsGodotArray())
                    {
                        try { mgr2.RestorePendingReturn(ModelId.Deserialize(rv.AsString())); }
                        catch (Exception e) { Log.Warn($"[Sarkaz] 跳过无法解析的待返还思绪: {e.Message}"); }
                    }
                }
            }

            // 待发构想 / 金币加成：只在“战斗结束→发奖”之间的存档里出现。
            // 读档走 StartPreFinishedCombat 重新发奖(不触发 AfterRoomEntered 的清零)，恢复后会被正常消费。
            if (pd.ContainsKey("pending_plan_rewards"))
            {
                player.GetSarkazManager()?.RestorePendingPlanRewards(pd["pending_plan_rewards"].AsInt32());
            }

            if (pd.ContainsKey("pending_gold_multiplier"))
            {
                player.GetSarkazManager()?.RestorePendingGoldMultiplier(pd["pending_gold_multiplier"].AsSingle());
            }

            if (pd.ContainsKey("prev_act_was_fragment"))
            {
                SarkazManagerModel mgrFlag = player.GetSarkazManager();
                if (mgrFlag != null) mgrFlag.PreviousActWasFragment = pd["prev_act_was_fragment"].AsBool();
            }

            if (pd.ContainsKey("fated_encounter_done"))
            {
                SarkazManagerModel mgrFated = player.GetSarkazManager();
                if (mgrFated != null) mgrFated.FatedEncounterDone = pd["fated_encounter_done"].AsBool();
            }

            if (pd.ContainsKey("fragment_coords"))
            {
                SarkazManagerModel mgr3 = player.GetSarkazManager();
                if (mgr3 != null)
                {
                    foreach (Variant fv in pd["fragment_coords"].AsGodotArray())
                    {
                        Godot.Collections.Dictionary fd = fv.AsGodotDictionary();
                        mgr3.RestoreFragmentCoord(fd["act"].AsInt32(),
                            new MapCoord(fd["col"].AsInt32(), fd["row"].AsInt32()));
                    }
                }
            }
            return true;
        }
        return false;
    }

    private static Godot.Collections.Dictionary ThoughtToDict(ThoughtModel t)
    {
        return new Godot.Collections.Dictionary
        {
            { "id", t.Id.ToString() },
            { "status", (int)t.Status },
            { "rarity", t.Rarity }, // Rarity/Toil 可在局内变化(如 DaybreakSun)，需持久化
            { "toil", t.Toil },
        };
    }

    // 从存档字典重建一个可变思绪实例（含 Status）。失败返回 null。不负责挂到玩家身上。
    private static ThoughtModel BuildThought(Godot.Collections.Dictionary td)
    {
        if (!td.ContainsKey("id")) return null;
        ModelId id;
        try { id = ModelId.Deserialize(td["id"].AsString()); }
        catch (Exception e) { Log.Warn($"[Sarkaz] 跳过无法解析的思绪 id: {e.Message}"); return null; }

        ThoughtModel canonical = ModelDb.GetById<ThoughtModel>(id);
        if (canonical == null)
        {
            Log.Warn($"[Sarkaz] 存档中的思绪 {id} 已不存在，跳过");
            return null;
        }

        ThoughtModel thought = canonical.ToMutable();
        if (td.ContainsKey("status")) thought.Status = (ThoughtStatus)td["status"].AsInt32();
        if (td.ContainsKey("rarity")) thought.Rarity = td["rarity"].AsInt32();
        if (td.ContainsKey("toil")) thought.Toil = td["toil"].AsInt32();
        return thought;
    }

    public static void Delete()
    {
        if (FileAccess.FileExists(FilePath))
        {
            DirAccess.RemoveAbsolute(FilePath);
        }
    }

    private static Godot.Collections.Dictionary ReadRoot()
    {
        if (!FileAccess.FileExists(FilePath)) return null;
        using FileAccess f = FileAccess.Open(FilePath, FileAccess.ModeFlags.Read);
        if (f == null) return null;

        Variant parsed = Json.ParseString(f.GetAsText());
        return parsed.VariantType == Variant.Type.Dictionary ? parsed.AsGodotDictionary() : null;
    }
}
