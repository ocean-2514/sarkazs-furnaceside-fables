using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Cards;

namespace Skz.Ocean.Model.Relics;

// 196 支援地雷组：每次战斗开始时向你的牌堆中加入三张干扰地雷
public sealed class SupportLandmineSet : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<InterferenceMineCard>();

    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.AddCardsToDraw<InterferenceMineCard>(this, 3);
    }
}
