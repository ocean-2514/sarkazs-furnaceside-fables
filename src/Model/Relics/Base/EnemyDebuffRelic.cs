using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Skz.Ocean.Model.Relics.Base;

// “所有敌方单位获得 X”的遗物基类。
// 不用 BeforeCombatStart 一次性施加：年代之刺等敌人是第一回合才入场的，
// 改为每个我方回合开始时给"还没登记过"的存活敌人补挂——天然覆盖中途入场。
public abstract class EnemyDebuffRelic : RelicModel
{
    // 本场已处理过的敌人（战斗内瞬态，战斗结束清空）
    private readonly HashSet<Creature> _applied = new HashSet<Creature>();

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Creature.Side)
        {
            return;
        }
        foreach (Creature enemy in combatState.Enemies.ToList())
        {
            if (enemy == null || !enemy.IsAlive || _applied.Contains(enemy))
            {
                continue;
            }
            _applied.Add(enemy);
            await ApplyToEnemy(enemy);
        }
    }

    protected abstract Task ApplyToEnemy(Creature enemy);

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _applied.Clear();
        return Task.CompletedTask;
    }
}
