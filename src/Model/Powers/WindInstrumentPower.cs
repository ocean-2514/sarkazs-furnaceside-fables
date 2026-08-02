using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Hooks;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Powers;

public sealed class WindInstrumentPower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    private int _totalAttackPower = 0;

    private int _attackPowerBuff = 10;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        if (Owner.HasPower<BlockPower>()) return;
        if (_totalAttackPower >= 30) return;
        _totalAttackPower += _attackPowerBuff;
        await PowerCmd.Apply<AttackPower>(choiceContext, Owner, _attackPowerBuff, null, null);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        if (oldOwner == null) return;
        _totalAttackPower = 0;
        await PowerCmd.Apply<AttackPower>(new ThrowingPlayerChoiceContext(), oldOwner, -_totalAttackPower, null, null);
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature dealer, DamageResult result, ValueProp props, Creature target, CardModel cardSource)
    {
        if (dealer != Owner) return;
        await PowerCmd.Apply<AttackPower>(choiceContext, Owner, -_totalAttackPower, null, null);
        _totalAttackPower = 0;
    }
}