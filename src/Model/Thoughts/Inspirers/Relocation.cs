using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 025 离迁：使用后，下次战斗所有我方单位获得 50% 闪避。
public sealed class Relocation : ThoughtModel
{
    protected override int InitialRarity => 4;
    protected override int InitialToil => 5;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Rare;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (Status != ThoughtStatus.Used) return;
        if (side != Owner.Creature.Side || combatState.RoundNumber != 1) return;
        await ThoughtFx.ApplyToAllAllies<DodgePower>(combatState, Owner.Creature, 50);
    }
}