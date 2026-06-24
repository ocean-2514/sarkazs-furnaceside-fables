using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 052 淬刃：使用后下次战斗所有友方干员造成伤害时附带一层凋亡损伤。
public sealed class Envenom : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        Log.Info($"{target.Name} received {result.UnblockedDamage} from {dealer.Name}!");
        if (Status != ThoughtStatus.Used) return;
        if (dealer == null || dealer.Side != Owner.Creature.Side) return; // 我方造成
        if (target.Side == Owner.Creature.Side) return;                   // 对敌方
        if (!props.IsPoweredAttack()) return;                             // 攻击才附带
        if (result.UnblockedDamage <= 0) return;
        await PowerCmd.Apply<Necrosis>(target, 1, dealer, null);
    }
}