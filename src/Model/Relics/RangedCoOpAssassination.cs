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

// 052 远程协议-克敌：牌组每有一张攻击牌，攻击速度+2
public sealed class RangedCoOpAssassination : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task BeforeCombatStart()
    {
        Flash();
        int n = PileType.Deck.GetPile(Owner).Cards.Count(c => c.Type == CardType.Attack);
        if (n > 0) await SarkazRelicFx.ApplySelf<AttackSpeed>(this, n * 2);
    }
}
