using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts;

public sealed class MockThought : ThoughtModel
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(3)
    ];
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;

    public override int Rarity => 1;

    public override int Toil => 1;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == Owner.Creature.Side && combatState.RoundNumber <= 1)
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
    }
}