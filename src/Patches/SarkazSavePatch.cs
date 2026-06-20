using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Saves;

namespace Skz.Ocean.Patches;

// 写侧存档：游戏每次把 run 序列化成 SerializableRun(RunManager.ToSave)时，
// 同步把萨卡兹数据(护盾/负荷上限/思绪列表)写到侧存档。仅单人。
[HarmonyPatch(typeof(RunManager), nameof(RunManager.ToSave))]
public static class SarkazRunSavePatch
{
    public static void Postfix(RunManager __instance)
    {
        try
        {
            if (__instance.NetService == null || __instance.NetService.Type != NetGameType.Singleplayer) return;
            // State 是 private 属性，读它拿当前活动 RunState（SerializableRun 里没有 PlayerExtraData）。
            RunState state = Traverse.Create(__instance).Property("State").GetValue<RunState>();
            if (state == null) return;
            SarkazSaveIO.Save(state);
        }
        catch (Exception e)
        {
            Log.Error($"[Sarkaz] 写侧存档失败: {e}");
        }
    }
}

// 读侧存档：读档重建每个 Player 后，挂上 manager 并按 NetId 恢复萨卡兹数据。
// 新开局走 RunState.CreateForNewRun(不经此处)，由 GrantStartingThoughtsPatch 处理。
[HarmonyPatch(typeof(Player), nameof(Player.FromSerializable))]
public static class SarkazPlayerLoadPatch
{
    public static void Postfix(Player __result)
    {
        try
        {
            __result.EnsureSarkazManager();
            SarkazSaveIO.RestoreToPlayer(__result);
        }
        catch (Exception e)
        {
            Log.Error($"[Sarkaz] 读侧存档失败: {e}");
        }
    }
}
