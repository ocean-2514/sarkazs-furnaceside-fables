using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
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
            ModHelper.SubscribeForCombatStateHooks("ocean.sarkaz.thoughts",
                combatState => CollectSarkazCombatModels(combatState.RunState));

            // 若 LocManager 已构造（mod 初始化晚于它），这里立即注入 thoughts 表；
            // 否则初始 SetLanguage 会被 ThoughtsLocPatch 拦到。语言切换由 patch 处理。
            Patches.ThoughtsLocPatch.InjectIntoCurrent();

            // 把 mod 遗物按稀有度注册进奖励池（必须在游戏初始化冻结池之前）。
            RegisterRelicPools();

            // [SavedProperty] 持久化：SavedPropertiesTypeCache 的静态构造器只扫描
            // 原版的 AbstractModelSubtypes 清单，mod 类型不注入的话 SavedProperties.From
            // 会静默返回空（存档丢字段）。官方为 mod 留了 InjectTypeIntoCache 接口。
            RegisterSavedProperties();
        }
        catch (Exception e)
        {
            Log.Error($"An error occurred while initializing Sarkaz mod: {e}");
            return;
        }

        Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(Assembly.GetExecutingAssembly());

        Log.Info("sarkaz mod initialize");
    }

    // Common/Uncommon/Rare/Shop → SharedRelicPool（战斗/宝箱/商店奖励）；
    // Event → EventRelicPool（只由事件发放）。None/Starter/Ancient 不进池。
    private static void RegisterRelicPools()
    {
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.IsAbstract || !typeof(RelicModel).IsAssignableFrom(type)) continue;
            RelicModel probe;
            try
            {
                probe = (RelicModel)Activator.CreateInstance(type);
            }
            catch (Exception e)
            {
                Log.Error($"[Sarkaz] 无法实例化遗物 {type.Name} 以读取稀有度: {e.Message}");
                continue;
            }
            switch (probe.Rarity)
            {
                case RelicRarity.Common:
                case RelicRarity.Uncommon:
                case RelicRarity.Rare:
                case RelicRarity.Shop:
                    ModHelper.AddModelToPool(typeof(SharedRelicPool), type);
                    break;
                case RelicRarity.Event:
                    ModHelper.AddModelToPool(typeof(EventRelicPool), type);
                    break;
            }
        }
    }

    private static void RegisterSavedProperties()
    {
        foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (type.IsAbstract || !typeof(AbstractModel).IsAssignableFrom(type)) continue;
            MegaCrit.Sts2.Core.Saves.Runs.SavedPropertiesTypeCache.InjectTypeIntoCache(type);
        }
    }

    private static IEnumerable<AbstractModel> CollectSarkazModels(IRunState runState)
    {
        List<AbstractModel> list = new List<AbstractModel>();
        foreach (Player player in runState.Players)
        {
            // 仅遗愿(Linger)参与钩子；灵感/构想不接。
            list.AddRange(player.GetLingers());
            if (player.GetSarkazManager() is { } mgr) list.Add(mgr);
        }
        return list;
    }
    
    private static IEnumerable<AbstractModel> CollectSarkazCombatModels(IRunState runState)
    {
        List<AbstractModel> list = new List<AbstractModel>();
        foreach (Player player in runState.Players)
        {
            if (player.GetThoughtUsed() is { } used) list.Add(used);
            if (player.GetCombatManager() is { } combatMgr) list.Add(combatMgr);
        }
        return list;
    }
}
