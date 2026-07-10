using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts.Interpreter;
using Skz.Ocean.Factories;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Relics;

// 166 彩虹瓮：受到敌人攻击时，将弃牌堆中随机一张牌放入你的手牌，每回合最多触发3次
public sealed class RainbowJar : RelicModel
{
    private int _triggersThisTurn;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (target != Owner.Creature || dealer == null || !dealer.IsEnemy) return;
        if (!props.IsPoweredAttack() || _triggersThisTurn >= 3) return;
        List<CardModel> discard = PileType.Discard.GetPile(Owner).Cards.ToList();
        if (discard.Count == 0) return;
        _triggersThisTurn++;
        Flash();
        CardModel card = Owner.RunState.Rng.CombatCardSelection.NextItem(discard);
        await CardPileCmd.Add(card, PileType.Hand);
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Creature.Side) _triggersThisTurn = 0;
        return Task.CompletedTask;
    }
}
