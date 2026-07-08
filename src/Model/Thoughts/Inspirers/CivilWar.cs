using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 044 内战：使用后下次战斗中所有能力(Power)和技能(Skill)牌费用 -1。
public sealed class CivilWar : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 1;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Rare;

    private int turnLimit = 2;

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (turnLimit <= 0)
        {
            modifiedCost = originalCost;
            return false;
        }
        modifiedCost = originalCost;
        if (Status != ThoughtStatus.Used || card.Owner != Owner) return false;
        if (card.Type != CardType.Power && card.Type != CardType.Skill) return false;
        modifiedCost = Math.Max(0, originalCost - 1);
        return true;
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Creature.Side && Status == ThoughtStatus.Used) turnLimit--;
        return Task.CompletedTask;
    }

}