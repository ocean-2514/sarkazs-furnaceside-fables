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

// 169 无惧之刃：手牌中费用最高的攻击牌造成的伤害+100%，每回合生效1次
public sealed class FearlessBlade : RelicModel
{
    private bool _usedThisTurn;
    private CardModel _boostedCard;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override decimal ModifyDamageAdditive(Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (dealer != Owner.Creature || !props.IsPoweredAttack()) return 0m;
        if (cardSource == null || cardSource.Type != CardType.Attack) return 0m;
        if (cardSource.EnergyCost.CostsX) return 0m;
        // 同一次打出的多段伤害都吃加成；本回合已用于其他牌则不再生效
        if (_usedThisTurn && cardSource != _boostedCard) return 0m;
        decimal cost = cardSource.EnergyCost.GetWithModifiers(CostModifiers.Local);
        bool isHighest = PileType.Hand.GetPile(Owner).Cards
            .Where(c => c.Type == CardType.Attack && !c.EnergyCost.CostsX)
            .All(c => c.EnergyCost.GetWithModifiers(CostModifiers.Local) <= cost);
        if (!isHighest) return 0m;
        _usedThisTurn = true;
        _boostedCard = cardSource;
        return amount;
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Creature.Side)
        {
            _usedThisTurn = false;
            _boostedCard = null;
        }
        return Task.CompletedTask;
    }
    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<FearlessBladePower>(this, 1);
    }
}
