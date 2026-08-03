using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Entities.Players;

namespace Skz.Ocean.Model.Powers;

public sealed class RewardPlanPower : PowerModel, ISarkazModel
{
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerType Type => PowerType.Buff;

    // 次要敌人（同原版 MinionPower）：CombatManager.IsEnding 只看存活的主要敌人，
    // 所以年代之刺不阻挡战斗结束；最后一个主要敌人死亡时它会被清场击杀。
    public override bool OwnerIsSecondaryEnemy => true;

    public override bool ShouldPowerBeRemovedAfterOwnerDeath() => false;

    public override bool ShouldOwnerDeathTriggerFatal() => false;

    public override Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature dealer, DamageResult result, ValueProp props, Creature target, CardModel cardSource)
    {
        if (target != Owner) return Task.CompletedTask;
        if (!result.WasTargetKilled) return Task.CompletedTask;
        if (dealer == null) return Task.CompletedTask;
        if (dealer.Side == CombatSide.Enemy) return Task.CompletedTask;
        foreach (Player player in CombatState.Players)
        {
            player.GetSarkazManager()?.RegisterPlanReward();
        }
        return Task.CompletedTask;
    }
}
