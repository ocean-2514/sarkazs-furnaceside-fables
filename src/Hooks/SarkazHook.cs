using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Entities.Thoughts.Interpreter;
using Skz.Ocean.Model;
using Skz.Ocean.Runs;

namespace Skz.Ocean.Hooks;

public static class SarkazHook
{
    public static MapPointType ModifyRefreshedPointType(IRunState runState, MapPointType pointType, MapPointType initialType)
    {
        MapPointType type = pointType;
        foreach (ISarkazModel model in runState.IterateSarkazHookListeners(null).ToList())
        {
            type = model.ModifyRefreshedPointType(runState, type, initialType);
        }
        return type;
    }

    public static async Task AfterThoughtUsed(IRunState runState, ThoughtModel thought)
    {
        foreach (ISarkazModel model in runState.IterateSarkazHookListeners(null).ToList())
        {
            await model.AfterThoughtUsed(runState, thought);
        }
    }
    
    public static async Task AfterDamageDodged(IRunState runState, ICombatState? combatState, Creature target, decimal amount, Creature? dealer)
    {
        foreach (ISarkazModel model in runState.IterateSarkazHookListeners(combatState).ToList())
        {
            await model.AfterDamageDodged(runState, combatState, target, amount, dealer);
        }
    }

    public static async Task AfterInterpreted(IRunState runState, Player player, InterpretationResult result)
    {
        foreach (ISarkazModel model in runState.IterateSarkazHookListeners(null).ToList())
        {
            await model.AfterInterpreted(runState, player, result);
        }
    }

    // 链式累加：把上一个监听者的结果传给下一个（勿传原值）。
    public static decimal ModifyMonsterMoveDelta(IRunState runState, ICombatState combatState,
        Model.Monsters.Base.SarkazMonsterModel monster, decimal delta)
    {
        decimal current = delta;
        foreach (ISarkazModel model in runState.IterateSarkazHookListeners(combatState).ToList())
        {
            current = model.ModifyMonsterMoveDelta(runState, combatState, monster, current);
        }
        return current;
    }

    // 怪物逃离战斗后广播（波次 encounter 推进出怪、遗物结算等）。
    public static async Task AfterMonsterEscaped(IRunState runState, ICombatState combatState,
        Model.Monsters.Base.SarkazMonsterModel monster)
    {
        foreach (ISarkazModel model in runState.IterateSarkazHookListeners(combatState).ToList())
        {
            await model.AfterMonsterEscaped(runState, combatState, monster);
        }
    }

    // 乘法合成：各监听者返回倍率，连乘后作用到基础移速（原版 ModifyDamageMultiplicative 同款语义）。
    public static decimal ModifyMoveSpeedMultiplicative(IRunState runState, ICombatState combatState,
        Model.Monsters.Base.SarkazMonsterModel monster, decimal baseSpeed)
    {
        decimal factor = 1m;
        foreach (ISarkazModel model in runState.IterateSarkazHookListeners(combatState).ToList())
        {
            factor *= model.ModifyMoveSpeedMultiplicative(runState, combatState, monster, baseSpeed);
        }
        return Math.Max(0, baseSpeed * factor);
    }
}