using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Entities.Thoughts.Interpreter;

namespace Skz.Ocean.Model;

public interface ISarkazModel
{
    public virtual Task AfterThoughtUsed(IRunState runState, ThoughtModel thought)
    {
        return Task.CompletedTask;
    }
    
    public virtual Task AfterDamageDodged(IRunState runState, ICombatState? combatState, Creature target, decimal amount, Creature? dealer)
    {
        return Task.CompletedTask;
    }

    public virtual MapPointType ModifyRefreshedPointType(IRunState runState, MapPointType pointType, MapPointType initialType)
    {
        return pointType;
    }

    // 解读完成（两张思绪已消耗、结果已发放）后触发。
    public virtual Task AfterInterpreted(IRunState runState, Player player, InterpretationResult result)
    {
        return Task.CompletedTask;
    }
}