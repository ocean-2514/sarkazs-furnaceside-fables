using MegaCrit.Sts2.Core.Entities.Creatures;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 024 火海：使用后，下次战斗所有我方单位攻击速度 +35。
public sealed class Flames : ThoughtModel
{
    protected override int InitialRarity => 3;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Rare;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Status != ThoughtStatus.Used) return;
        if (side != Owner.Creature.Side || combatState.RoundNumber != 1) return;
        await ThoughtFx.ApplyToAllAllies<AttackSpeed>(combatState, Owner.Creature, 35);
    }
}