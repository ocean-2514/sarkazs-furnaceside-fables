using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Helper;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 032 取食：使用后下次战斗所有我方单位最大生命值 +15%（仅本场）。
public sealed class Consume : ThoughtModel
{
    protected override int InitialRarity => 1;
    protected override int InitialToil => 1;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;
    public override bool CanBeUpgraded => true;
    public override Type UpgradedType => typeof(Reclaim);

    private Dictionary<Creature, int> _granted;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Status != ThoughtStatus.Used) return;
        if (side != Owner.Creature.Side || combatState.RoundNumber != 1) return;
        _granted = await ThoughtFx.GainTempMaxHpPercent(combatState, 0.15);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await ThoughtFx.LoseTempMaxHp(_granted);
        _granted = null;
        await base.AfterCombatEnd(room);
    }
}