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

// 048 突击协议-利刃：牌组每有一张攻击牌，攻击力+2%
public sealed class AssaultCoOpSharpBlade : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override async Task BeforeCombatStart()
    {
        Flash();
        int n = PileType.Deck.GetPile(Owner).Cards.Count(c => c.Type == CardType.Attack);
        if (n > 0) await SarkazRelicFx.ApplySelf<AttackPower>(this, n * 2);
    }
}
