using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Cards;

namespace Skz.Ocean.Model.Relics;

// 200 支援雾机：每次战斗开始时向你的牌堆中加入一张支援雾机
public sealed class SupportFogMachine : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromCardWithCardHoverTips<FogMachineCard>();

    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.AddCardsToDraw<FogMachineCard>(this, 1);
    }
}
