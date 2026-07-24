using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Cards;

namespace Skz.Ocean.Model.Relics;

// 199 支援防暴桩：每次战斗开始时向你的牌堆中加入两张便携式防暴桩
public sealed class SupportRiotPile : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<RiotPileCard>();

    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.AddCardsToDraw<RiotPileCard>(this, 2);
    }
}
