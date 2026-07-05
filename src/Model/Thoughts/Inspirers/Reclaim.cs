using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Helper;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 033 垦荒：使用后下次战斗所有我方单位最大生命值 +30%（仅本场）。
public sealed class Reclaim : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;
    // public override bool CanBeUpgraded => true;
    // public override Type UpgradedType => typeof(RiseAndFall);

    private Dictionary<Creature, int> _granted;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (Status != ThoughtStatus.Used) return;
        if (side != Owner.Creature.Side || combatState.RoundNumber != 1) return;
        _granted = await ThoughtFx.GainTempMaxHpPercent(combatState, 0.30);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await ThoughtFx.LoseTempMaxHp(_granted);
        _granted = null;
        await base.AfterCombatEnd(room);
    }
}