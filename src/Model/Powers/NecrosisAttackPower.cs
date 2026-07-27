using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Runs;

namespace Skz.Ocean.Model.Powers;

public sealed class NecrosisAttackPower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (dealer == null || dealer.Side != Owner.Side) return; 
        if (target.Side == Owner.Side) return;                   // 对敌方
        if (!props.IsPoweredAttack()) return;                             // 攻击才附带
        await PowerCmd.Apply<Necrosis>(choiceContext, target, Amount, dealer, null);
    }
}