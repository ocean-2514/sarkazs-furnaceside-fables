using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Skz.Ocean.Model.Powers;

public sealed class AttackPower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // 敌方攻击力-x%（开裂的束缚带等）与未叙魔王残片都需要负层数
    public override bool AllowNegative => true;

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (dealer != Owner) return 0m;
        if (!props.IsPoweredAttack()) return 0m;
        if (Owner.Side == CombatSide.Player)
        {
            if (cardSource == null) return 0m;
            if (cardSource.Type != CardType.Attack) return 0m;
            return (amount * (decimal)Amount / 100m);
        }
        else
        {
            return (amount * (decimal)Amount / 100m);
        }
    }
}