using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Skz.Ocean.Model.Powers;

public sealed class ColdPower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    private const decimal _decreasedAttackSpeed = 20;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        await PowerCmd.Decrement(this);
    }

    public override async Task AfterApplied(Creature applier, CardModel cardSource)
    {
        await PowerCmd.Apply<AttackSpeed>(new ThrowingPlayerChoiceContext(), Owner, -_decreasedAttackSpeed, applier, cardSource);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await PowerCmd.Apply<AttackSpeed>(new ThrowingPlayerChoiceContext(), Owner, _decreasedAttackSpeed, null, null);
    }
}