using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Skz.Ocean.Model.Powers;

public sealed class NecrosisBurst : PowerModel, ISarkazModel
{
    private int _enemyHpLoss = 10;
    private int _playerHpLoss = 5;

    private decimal _damageMultiplier = 0.5m;

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature applier, out decimal modifiedAmount)
    {
        if (target != Owner)
        {
            modifiedAmount = amount;
            return false;
        }
        if (canonicalPower is Necrosis)
        {
            modifiedAmount = 0;
            return true;
        }
        modifiedAmount = amount;
        return false;
    }

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (side == Owner.Side)
        {
            if (Owner.Player != null)
            {
                await PlayerCmd.LoseEnergy(1m, Owner.Player);
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner, _playerHpLoss, 
                    ValueProp.Unblockable | ValueProp.Unpowered, null, null);
            } 
            else
            {
                await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner, _enemyHpLoss, 
                    ValueProp.Unblockable | ValueProp.Unpowered, null, null);
            }
        }
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side) return;
        await PowerCmd.Decrement(this);
    }

    public override decimal ModifyDamageMultiplicative(Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (dealer != Owner) return 1.0m;
        if (!props.IsPoweredAttack()) return 1.0m;
        if (Owner.Player != null) return 1.0m;
        return _damageMultiplier;
    }
}