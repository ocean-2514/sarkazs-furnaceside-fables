using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Entities.Players;

namespace Skz.Ocean.Model.Powers;

public sealed class BorrowShieldPower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature dealer, DamageResult result, ValueProp props, Creature target, CardModel cardSource)
    {
        if (dealer != Owner || target.Player == null) return;
        int _borrowed = Math.Min(Amount, target.Player.GetShield());
        if (_borrowed > 0)
        {
            target.Player.LoseShield(_borrowed);
            await CreatureCmd.GainBlock(Owner, _borrowed, ValueProp.Unpowered, null);
        }
    }
}