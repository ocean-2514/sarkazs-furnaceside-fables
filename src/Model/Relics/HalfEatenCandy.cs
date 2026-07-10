using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Relics;

// 047 没吃完的糖果：第一回合开始时选择一张无色牌加入手牌，负荷临界点+2
public sealed class HalfEatenCandy : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        Owner.IncreaseToilLimit(2);
        await Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;
        if (Owner.Creature?.CombatState is not { } cs || cs.RoundNumber != 1) return;
        List<CardModel> pool = ModelDb.CardPool<ColorlessCardPool>()
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .ToList();
        if (pool.Count == 0) return;
        List<CardModel> options = new List<CardModel>();
        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            CardModel pick = Owner.RunState.Rng.Niche.NextItem(pool);
            pool.Remove(pick);
            options.Add(pick);
        }
        CardModel chosen = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner, canSkip: true);
        if (chosen == null) return;
        CardModel card = Owner.Creature.CombatState.CreateCard(chosen, Owner);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }
}
