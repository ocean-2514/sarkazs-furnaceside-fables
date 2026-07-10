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

// 115 掠食之手：打出技能时攻击力+10%（最多10层）；拥有钝爪-掠食时，每2个回合获得能量
public sealed class HandOfPredation : RelicModel
{
    private decimal _applied;
    private int _turnCounter;

    public override RelicRarity Rarity => RelicRarity.Rare;

    [SavedProperty]
    public int TurnCounter
    {
        get => _turnCounter;
        set
        {
            AssertMutable();
            _turnCounter = value;
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || !CombatManager.Instance.IsInProgress) return;
        if (cardPlay.Card.Type != CardType.Skill || _applied >= 100) return;
        _applied += 10;
        await SarkazRelicFx.ApplySelf<AttackPower>(this, 10);
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Creature.Side) return;
        if (!Owner.Relics.Any(r => r is BluntClawsPredation)) return;
        TurnCounter++;
        if (TurnCounter >= 2)
        {
            TurnCounter = 0;
            Flash();
            await PlayerCmd.GainEnergy(1, Owner);
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _applied = 0;
        return Task.CompletedTask;
    }
    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<HandOfPredationPower>(this, 1);
    }
}
