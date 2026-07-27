using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Entities.Thoughts.Interpreter;
using Skz.Ocean.Model.Monsters.Base;

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

    // 修改 Sarkaz 怪物本回合的位移量（初值 = 当前移速）。传送/击退类一次性效果
    // 在这里改 delta；返回值会被下一个监听者继续累加修正，最终 clamp 到 ≥0 后生效。
    public virtual decimal ModifyMonsterMoveDelta(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster, decimal delta)
    {
        return delta;
    }

    // 返回作用于 Sarkaz 怪物移速的乘数（默认 1，不改变）。减速/加速等持续效果在
    // 这里返回倍率（如减速 30% 返回 0.7m），各监听者的乘数连乘后作用到基础移速。
    // baseSpeed 仅供参考（怪物的基础移速）。
    public virtual decimal ModifyMoveSpeedMultiplicative(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster, decimal baseSpeed)
    {
        return 1m;
    }
    
    public virtual decimal ModifyMoveSpeedAdditive(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster, decimal baseSpeed)
    {
        return 0m;
    }

    // Sarkaz 怪物进蓝门逃离战斗后触发（此时它已从敌人列表移除）。
    // 波次型 encounter 靠它推进出怪；遗物/思绪也可借此结算"敌人逃走"效果。
    public virtual Task AfterMonsterEscaped(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster)
    {
        return Task.CompletedTask;
    }
    
    
    public virtual Task AfterMonsterBlocked(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster)
    {
        return Task.CompletedTask;
    }
}