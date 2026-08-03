using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Skz.Ocean.Model.Powers;

public sealed class InterferenceMinePower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    private int _timer = 1;

    private decimal _damage = 10;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return;
        _timer--;
        if (_timer == 0)
        {
            if (Owner.IsDead) return;
            await CreatureCmd.Damage(choiceContext, Owner, _damage, ValueProp.Unpowered | ValueProp.Unblockable, Applier);
            await PowerCmd.Remove(this);
        }
    }
}