using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts.Interpreter;
using Skz.Ocean.Factories;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Relics;

// 020 至宝指环：战斗掉落的金币+50%（在金币奖励基础上追加一半）
public sealed class SupremeRing : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom room)
    {
        if (player != Owner || room is not CombatRoom) return false;
        List<GoldReward> golds = rewards.OfType<GoldReward>().Where(g => g.Amount > 0).ToList();
        if (golds.Count == 0) return false;
        foreach (GoldReward gold in golds)
        {
            rewards.Add(new GoldReward(gold.Amount / 2, player));
        }
        return true;
    }
}
