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

// 141 灾难之源：所有敌方单位获得20%易伤，受到的元素（凋亡）损伤+100%
// 凋亡损伤识别：无来源(dealer==null)且 Unblockable|Unpowered 的伤害（NecrosisBurst 的施加方式）
public sealed class DisastersOrigin : EnemyDebuffRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override async Task ApplyToEnemy(Creature enemy)
    {
        await SarkazRelicFx.ApplyPower<InjuredPower>(this, enemy, 20);
    }

    public override decimal ModifyDamageAdditive(Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (target == null || !target.IsEnemy || dealer != null) return 0m;
        if (!props.HasFlag(ValueProp.Unblockable) || !props.HasFlag(ValueProp.Unpowered)) return 0m;
        return amount;
    }
    // public override async Task BeforeCombatStart()
    // {
    //     await SarkazRelicFx.ApplySelf<DisastersOriginPower>(this, 1);
    // }
}
