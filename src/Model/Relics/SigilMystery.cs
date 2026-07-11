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
using Skz.Ocean.Map;
using Skz.Ocean.Model.Acts;
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Model.Relics.Base;

namespace Skz.Ocean.Model.Relics;

// 163 烙印之谜：第一回合所有牌费用为0，第二回合开始你打出的前1张牌费用+1，仅诡谲断章内生效。所有敌人生命值+10%。
public sealed class SigilMystery : FragmentMysteryRelic
{
    private int _playedThisTurn;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (!InFragment || card.Owner != Owner) return false;
        if (Owner.Creature?.CombatState is not { } cs) return false;
        if (cs.RoundNumber == 1)
        {
            modifiedCost = 0;
            return true;
        }
        if (_playedThisTurn < 1)
        {
            modifiedCost = originalCost + 1;
            return true;
        }
        return false;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner) _playedThisTurn++;
        return Task.CompletedTask;
    }

    protected override Task OnPlayerTurnStartInFragment(ICombatState combatState)
    {
        _playedThisTurn = 0;
        return Task.CompletedTask;
    }
}
