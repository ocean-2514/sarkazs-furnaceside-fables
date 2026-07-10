using System.Collections.Generic;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Model;

namespace Skz.Ocean.Runs;

public static class IRunStateExtensions
{
    public static IEnumerable<ISarkazModel> IterateSarkazHookListeners(this IRunState runState, ICombatState combatState)
    {
        foreach (Player player in runState.Players)
        {
            foreach (ThoughtModel thought in player.GetAllThoughts())
            {
                yield return thought;
            }
            // 已使用的灵感：使用时已从三大列表移除，但战斗期间仍在生效，必须能收到钩子
            if (player.GetThoughtUsed() is { } used)
            {
                yield return used;
            }
            // 常驻 manager（与 vanilla 钩子的 CollectSarkazModels 保持一致）
            if (player.GetSarkazManager() is { } manager)
            {
                yield return manager;
            }
            if (player.GetCombatManager() is { } combatManager)
            {
                yield return combatManager;
            }
            foreach (RelicModel relic in player.Relics)
            {
                if (relic is ISarkazModel model)
                {
                    yield return model;
                }
            }
        }
        if (combatState != null)
        {
            foreach (Creature creature in combatState.Enemies)
            {
                foreach (PowerModel power in creature.Powers)
                {
                    if (power is ISarkazModel model)
                    {
                        yield return model;
                    }
                }
            }
            foreach (Creature creature in combatState.Allies)
            {
                foreach (PowerModel power in creature.Powers)
                {
                    if (power is ISarkazModel model)
                    {
                        yield return model;
                    }
                }
            }
        }
    }
}
