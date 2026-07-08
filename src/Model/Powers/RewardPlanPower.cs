using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
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

    public override Task BeforeDeath(Creature creature)
    {
        if (creature != Owner) return Task.CompletedTask;

        // 清场击杀（主要敌人死光后 CreatureCmd 对残余次要敌人统一 Kill）不算击败：
        // 只有死亡瞬间场上仍有存活的主要敌人，才是玩家主动击杀，才给构想。
        bool playerKill = CombatState.Enemies.Any(e => e != null && e.IsAlive && e.IsPrimaryEnemy);
        if (!playerKill) return Task.CompletedTask;

        // 不走 CombatRoom.ExtraRewards：它会被序列化，而 PlanReward 的 RewardType 占位是
        // Relic，奖励界面 SL 后会被还原成随机遗物奖励。改记到 manager 的待发计数，
        // 发奖时经 TryModifyRewards 追加，计数由侧存档持久化。
        foreach (Player player in CombatState.Players)
        {
            player.GetSarkazManager()?.RegisterPlanReward();
        }
        return Task.CompletedTask;
    }
}
