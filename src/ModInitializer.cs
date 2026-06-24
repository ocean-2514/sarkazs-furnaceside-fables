using System;
using System.Collections.Generic;
using System.Reflection;
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
                runState => CollectSarkazModels(runState));
            // ModHelper.SubscribeForCombatStateHooks("ocean.sarkaz.thoughts",
            //     combatState => CollectSarkazModels(combatState.RunState));

            // 若 LocManager 已构造（mod 初始化晚于它），这里立即注入 thoughts 表；
            // 否则初始 SetLanguage 会被 ThoughtsLocPatch 拦到。语言切换由 patch 处理。
            Patches.ThoughtsLocPatch.InjectIntoCurrent();
        }
        catch (Exception e)
        {
            Log.Error($"An error occurred while initializing Sarkaz mod: {e}");
            return;
        }

        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());

        Log.Info("sarkaz mod initialize");
    }

    private static IEnumerable<AbstractModel> CollectSarkazModels(IRunState runState)
    {
        List<AbstractModel> list = new List<AbstractModel>();
        foreach (Player player in runState.Players)
        {
            // 仅遗愿(Linger)参与战斗钩子；灵感/构想不接。
            list.AddRange(player.GetLingers());
            // 这两个可能为 null（尚未使用思绪 / 开局补丁尚未赋予 manager）——过滤掉，
            // 否则钩子分发器遍历时会对 null 调方法。
            if (player.GetThoughtUsed() is { } used) list.Add(used);
            if (player.GetSarkazManager() is { } mgr) list.Add(mgr);
        }
        return list;
    }
}
