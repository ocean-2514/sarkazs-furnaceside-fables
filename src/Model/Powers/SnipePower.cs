using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Skz.Ocean.Model.Powers;

public sealed class SnipePower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (target != Owner || !target.IsAlive) return;
        // 只对"有威力加成的攻击"触发；追加的真实伤害是 Unpowered，不会递归触发自身
        if (!props.IsPoweredAttack()) return;
        await CreatureCmd.Damage(choiceContext, target, Amount,
            ValueProp.Unblockable | ValueProp.Unpowered, Owner);
    }
}