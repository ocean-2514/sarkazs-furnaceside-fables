using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Skz.Ocean.Model.Powers;

public sealed class MelancholicFlutePower : MelancholicPower
{
    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        await PowerCmd.Apply<PassionateFlutePower>(new ThrowingPlayerChoiceContext(), oldOwner, StackNum, null, null);
    }
}