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
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Model.Relics.Base;

namespace Skz.Ocean.Model.Relics;

// 102 古乔治营养原浆：生命值越高攻击力越高，100%生命值时最大+30%（对攻击牌的强化伤害，随当前生命线性）
public sealed class OldGeorgeNutritionalPaste : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override decimal ModifyDamageAdditive(Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (dealer != Owner.Creature) return 0m;
        if (!props.IsPoweredAttack()) return 0m;
        if (cardSource == null || cardSource.Type != CardType.Attack) return 0m;
        decimal ratio = Owner.Creature.MaxHp > 0 ? Owner.Creature.CurrentHp / Owner.Creature.MaxHp : 0m;
        return amount * 0.30m * ratio;
    }
}
