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

// 118 奥秘之手：每打出两张技能牌，获得1点能量，每回合最多触发4次
public sealed class HandOfMystery : RelicModel
{
    private int _skillsThisTurn;
    private int _triggersThisTurn;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner || !CombatManager.Instance.IsInProgress) return;
        if (cardPlay.Card.Type != CardType.Skill) return;
        _skillsThisTurn++;
        if (_skillsThisTurn % 2 == 0 && _triggersThisTurn < 4)
        {
            _triggersThisTurn++;
            Flash();
            await PlayerCmd.GainEnergy(1, Owner);
        }
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Creature.Side)
        {
            _skillsThisTurn = 0;
            _triggersThisTurn = 0;
        }
        return Task.CompletedTask;
    }
    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<HandOfMysteryPower>(this, 1);
    }
}
