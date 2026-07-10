using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Map;
using Skz.Ocean.Model.Acts;
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Model.Relics.Base;

namespace Skz.Ocean.Model.Relics;

// 160 猜疑之谜：战斗后获得的金币翻倍，但战斗中损失的生命值翻倍，仅诡谲断章内生效。所有敌人生命值+10%。
public sealed class SuspicionMystery : FragmentMysteryRelic
{
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (!InFragment || target != Owner.Creature || amount <= 0) return amount;
        if (dealer == null || dealer.Side != CombatSide.Enemy) return amount;
        return amount * 2;
    }

    public override bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom room)
    {
        if (!InFragment || player != Owner || room is not CombatRoom) return false;
        List<GoldReward> golds = rewards.OfType<GoldReward>().Where(g => g.Amount > 0).ToList();
        if (golds.Count == 0) return false;
        foreach (GoldReward gold in golds)
        {
            rewards.Add(new GoldReward(gold.Amount, player));
        }
        return true;
    }
    public override async Task BeforeCombatStart()
    {
        if (!InFragment) return;
        await SarkazRelicFx.ApplySelf<SuspicionMysteryPower>(this, 1);
    }
}
