using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;

namespace Skz.Ocean.Model.Powers;

public sealed class DodgePower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return amount;
        if (amount <= 0) return amount;
        Rng rng = Owner.Player?.RunState.Rng.Niche ?? Owner.Monster.Rng;  
        return rng.NextInt(100) < Amount ? 0m : amount;
    }


}