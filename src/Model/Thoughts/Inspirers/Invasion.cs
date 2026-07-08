using MegaCrit.Sts2.Core.Entities.Creatures;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Powers;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Helper;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 035 侵略：使用后下次战斗所有我方单位获得 4 点力量。
public sealed class Invasion : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;

    // public override bool CanBeUpgraded => true;
    // public override Type UpgradedType => typeof(RiseAndFall);

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Status != ThoughtStatus.Used) return;
        if (side != Owner.Creature.Side || combatState.RoundNumber != 1) return;
        await ThoughtFx.ApplyToAllAllies<StrengthPower>(combatState, Owner.Creature, 4);
    }
}