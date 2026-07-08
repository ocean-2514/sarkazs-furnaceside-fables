using MegaCrit.Sts2.Core.Entities.Creatures;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 041 异族：使用后，下次战斗所有我方单位攻击速度 +15。
public sealed class Outblood : ThoughtModel
{
    protected override int InitialRarity => 1;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;

    public override bool CanBeUpgraded => true;
    public override Type UpgradedType => typeof(Flames);

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Status != ThoughtStatus.Used) return;
        if (side != Owner.Creature.Side || combatState.RoundNumber != 1) return;
        await ThoughtFx.ApplyToAllAllies<AttackSpeed>(combatState, Owner.Creature, 15);
    }
}