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
using MegaCrit.Sts2.Core.Models.Relics;
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

// 154 希望时代的涂鸦：金币+80，刷新节点时出现商店的概率小幅提高。获得[gold]送货员[/gold]和[gold]紫水晶茄子[/gold]和[gold]锈蚀的铁锤[/gold]
public sealed class DoodleOfHope : RelicModel, ISarkazModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        await PlayerCmd.GainGold(80, Owner);
        await RelicCmd.Obtain<TheCourier>(Owner);
        await RelicCmd.Obtain<AmethystAubergine>(Owner);
        await RelicCmd.Obtain<RustedIronHammer>(Owner);
    }

    public MapPointType ModifyRefreshedPointType(IRunState runState, MapPointType pointType, MapPointType initialType)
    {
        if (pointType == MapPointType.Shop || initialType == MapPointType.Shop) return pointType;
        Rng rng = (runState as RunState)?.Rng.Niche;
        if (rng != null && rng.NextFloat() < 0.5f) return MapPointType.Shop;
        return pointType;
    }

}
