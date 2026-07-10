using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Model.Relics.Base;

namespace Skz.Ocean.Model.Relics;

// 003 橙味风暴：第三回合多抽一张牌。攻击力+7%
public sealed class OrangeStorm : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner || player.Creature.CombatState.RoundNumber != 3) return count;
        Flash();
        return count + 1;
    }

    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<AttackPower>(this, 7);
    }
}
