using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Cards;

namespace Skz.Ocean.Model.Relics;

// 195 支援补给站：每次战斗开始时向你的牌堆中加入一张便携式补给站
public sealed class SupportReplenishmentStation : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<ReplenishmentStationCard>();

    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.AddCardsToDraw<ReplenishmentStationCard>(this, 1);
    }
}
