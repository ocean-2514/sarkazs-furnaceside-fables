using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Runs;

namespace Skz.Ocean.Model.Powers;

public sealed class Necrosis : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private const int MaxNecrosisAmount = 6;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this) return;
        if (Amount < MaxNecrosisAmount) return;
        await PowerCmd.Apply<NecrosisBurst>(choiceContext, Owner, 3m, applier, null);
        await PowerCmd.Remove(this);
    }
}