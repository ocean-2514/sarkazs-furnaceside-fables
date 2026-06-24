using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Powers;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Helper;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 045 休息：使用后下次战斗所有友方单位获得 6 层再生。
public sealed class RespiteInspirer : ThoughtModel
{
    protected override int InitialRarity => 1;
    protected override int InitialToil => 1;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;

    public override bool CanBeUpgraded => true;
    public override Type UpgradedType => typeof(Slumber);

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (Status != ThoughtStatus.Used) return;
        if (side != Owner.Creature.Side || combatState.RoundNumber != 1) return;
        await ThoughtFx.ApplyToAllAllies<RegenPower>(combatState, Owner.Creature, 6);
    }
}