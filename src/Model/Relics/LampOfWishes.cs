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

// 156 美愿时代的留恋：金币+40，刷新节点时更容易出现宝箱，打开宝箱时额外获得一件遗物
public sealed class LampOfWishes : RelicModel, ISarkazModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        await PlayerCmd.GainGold(40, Owner);
    }

    public MapPointType ModifyRefreshedPointType(IRunState runState, MapPointType pointType)
    {
        if (pointType == MapPointType.Treasure) return pointType;
        Rng rng = (runState as RunState)?.Rng.Niche;
        if (rng != null && rng.NextFloat() < 0.25f) return MapPointType.Treasure;
        return pointType;
    }

    // 宝箱房的额外奖励里追加一件遗物
    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom room)
    {
        if (player != Owner || room is not TreasureRoom) return false;
        rewards.Add(new RelicReward(player));
        return true;
    }
}
