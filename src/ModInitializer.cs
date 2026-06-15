using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Entities.Players;

namespace Skz.Ocean;

[ModInitializer(nameof(Initializer))]
class ModInitializer
{
    public static void Initializer()
    {
        try
        {
            var Harmony = new Harmony("ocean.sarkaz");
            Harmony.PatchAll();

            // 把思绪接进游戏的钩子系统，否则 ThoughtModel 的 AfterCombatEnd /
            // AfterSideTurnStart 等回调永远不会触发：
            //  - run 级钩子(如 AfterCombatEnd) 由 RunState.IterateHookListeners 分发
            //  - combat 级钩子(如 AfterSideTurnStart) 由 CombatState.IterateHookListeners 分发
            // 两者都会把 ModHelper 注册的订阅者纳入遍历。
            ModHelper.SubscribeForRunStateHooks("ocean.sarkaz.thoughts",
                runState => CollectThoughts(runState));
            ModHelper.SubscribeForCombatStateHooks("ocean.sarkaz.thoughts",
                combatState => CollectThoughts(combatState.RunState));
        }
        catch (Exception e)
        {
            Log.Error($"An error occurred while initializing Sarkaz mod: {e}");
            return;
        }

        Log.Info("sarkaz mod initialize");
    }

    // 返回 List 拷贝：回调里可能增删思绪（如 AfterCombatEnd 移除已用思绪），
    // 用拷贝避免遍历时修改集合。
    private static IEnumerable<AbstractModel> CollectThoughts(IRunState runState)
    {
        List<AbstractModel> list = new List<AbstractModel>();
        foreach (Player player in runState.Players)
        {
            list.AddRange(player.GetAllThoughts());
        }
        return list;
    }
}
