using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Skz.Ocean.Model.Powers;

public sealed class PassionateCelloPower : PassionatePower
{
    protected override int AttackSpeedBuff => 50;

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await base.AfterRemoved(oldOwner);
        await PowerCmd.Apply<MelancholicCelloPower>(new ThrowingPlayerChoiceContext(), oldOwner, StackNum, null, null);
    }
}