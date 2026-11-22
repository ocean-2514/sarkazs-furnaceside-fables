using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Skz.Ocean.Model.Powers;

public sealed class MelancholicWindPower : MelancholicPower
{
    protected override int AttackPowerBuff => 150;
    protected override int AttackSpeedDebuff => 50;

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        await PowerCmd.Apply<PassionateWindPower>(new ThrowingPlayerChoiceContext(), oldOwner, StackNum, null, null);
    }
}