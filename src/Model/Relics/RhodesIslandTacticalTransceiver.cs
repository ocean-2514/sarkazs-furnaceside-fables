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
using MegaCrit.Sts2.Core.Map;
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
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Model.Relics.Base;
using Skz.Ocean.Model.Thoughts.Inspirers;

namespace Skz.Ocean.Model.Relics;

// 122 罗德岛战术电台：战斗后掉落卡牌奖励时，增加一个可选项
public sealed class RhodesIslandTacticalTransceiver : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
    {
        if (player != Owner || cardRewardOptions.Count == 0) return false;
        // CreateForReward 结尾会再次触发 TryModifyCardRewardOptions 钩子，直接复用
        // creationOptions 会无限递归。克隆一份（WithFlags 是原地改，不能污染外层
        // 还在用的实例）并带上 NoModifyHooks，让内层生成跳过钩子。
        CardCreationOptions inner = (creationOptions with { }).WithFlags(CardCreationFlags.NoModifyHooks);
        cardRewardOptions.AddRange(CardFactory.CreateForReward(Owner, 1, inner));
        return true;
    }
}
