using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 046 安眠：使用后下次战斗所有友方单位每回合恢复 5% 最大生命值，持续 6 回合。
public sealed class Slumber : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Rare;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Status != ThoughtStatus.Used) return;
        if (side != Owner.Creature.Side || combatState.RoundNumber > 6) return;
        foreach (Creature ally in combatState.Allies.ToList())
        {
            int amount = (int)(ally.MaxHp * 0.05);
            if (amount > 0) await CreatureCmd.Heal(ally, amount);
        }
    }
}