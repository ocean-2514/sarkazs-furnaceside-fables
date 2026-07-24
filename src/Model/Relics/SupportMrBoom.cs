using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Cards;

namespace Skz.Ocean.Model.Relics;

// 197 支援轰隆隆：每次战斗开始时向你的牌堆中加入一张轰隆隆先生（Super Rare → Rare）
public sealed class SupportMrBoom : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<MrBoomCard>();

    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.AddCardsToDraw<MrBoomCard>(this, 1);
    }
}
