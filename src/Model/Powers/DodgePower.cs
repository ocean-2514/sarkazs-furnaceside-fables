using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Hooks;

namespace Skz.Ocean.Model.Powers;

public sealed class DodgePower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner) return amount;
        if (amount <= 0) return amount;
        if (dealer == null) return amount;
        if (dealer.Side != CombatSide.Enemy) return amount;
        Rng rng = Owner.Player?.RunState.Rng.Niche ?? Owner.Monster.Rng;
        if (rng.NextInt(100) < Amount)
        {
            // 同步 hook 里没法 await；RunSafely 包装让监听者异常进日志而不是被吞掉
			TalkCmd.Play(new LocString("vfx", "MISS"), Owner, VfxColor.Orange, VfxDuration.VeryShort);
            TaskHelper.RunSafely(SarkazHook.AfterDamageDodged(
                IRunState.GetFrom([Owner]), Owner.CombatState, Owner, amount, dealer));
            return 0m;
        }
        return amount; 
    }
}