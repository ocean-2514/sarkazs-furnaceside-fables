using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Model;
using Skz.Ocean.Runs;

namespace Skz.Ocean.Hooks;

public static class SarkazHook
{
    public static MapPointType ModifyRefreshedPointType(IRunState runState, MapPointType pointType)
    {
        MapPointType type = pointType;
        foreach (ISarkazModel model in runState.IterateSarkazHookListeners(null).ToList())
        {
            type = model.ModifyRefreshedPointType(runState, type);
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

    public static async Task AfterInterpreted(IRunState runState, MegaCrit.Sts2.Core.Entities.Players.Player player, Entities.Thoughts.Interpreter.InterpretationResult result)
    {
        foreach (ISarkazModel model in runState.IterateSarkazHookListeners(null).ToList())
        {
            await model.AfterInterpreted(runState, player, result);
        }
    }
}