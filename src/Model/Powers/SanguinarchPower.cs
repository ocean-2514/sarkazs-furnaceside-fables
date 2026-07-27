using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Skz.Ocean.Model.Powers;

public sealed class SanguinarchPower : PowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterApplied(Creature applier, CardModel cardSource)
    {
        foreach (Creature creature in Owner.CombatState.PlayerCreatures)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), creature, -1, null, null);
            await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), creature, -1, null, null);
        }
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        foreach (Creature creature in Owner.CombatState.PlayerCreatures)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), creature, 1, null, null);
            await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), creature, 1, null, null);
        }
    }
}
