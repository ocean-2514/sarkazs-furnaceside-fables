using MegaCrit.Sts2.Core.Entities.Creatures;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Powers;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Helper;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 023 筑墙：使用后，下次战斗所有我方单位部署后立刻获得 2 层缓冲。
public sealed class Wall : ThoughtModel
{
    protected override int InitialRarity => 3;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;

    public override bool CanBeUpgraded => true;
    public override Type UpgradedType => typeof(Relocation);

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Status != ThoughtStatus.Used) return;
        if (side != Owner.Creature.Side || combatState.RoundNumber != 1) return;
        await ThoughtFx.ApplyToAllAllies<BufferPower>(combatState, Owner.Creature, 2);
    }
}