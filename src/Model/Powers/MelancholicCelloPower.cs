using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Skz.Ocean.Model.Powers;

public sealed class MelancholicCelloPower : MelancholicPower
{
    protected override int AttackPowerBuff => 150;
    protected override int AttackSpeedDebuff => 50;

    private int _thornStack = 5;

    public override async Task AfterApplied(Creature applier, CardModel cardSource)
    {
        await base.AfterApplied(applier, cardSource);
        await PowerCmd.Apply<ThornsPower>(new ThrowingPlayerChoiceContext(), Owner, _thornStack, null, null);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        await PowerCmd.Apply<ThornsPower>(new ThrowingPlayerChoiceContext(), oldOwner, -_thornStack, null, null);
        await PowerCmd.Apply<PassionateCelloPower>(new ThrowingPlayerChoiceContext(), oldOwner, StackNum, null, null);
    }
}