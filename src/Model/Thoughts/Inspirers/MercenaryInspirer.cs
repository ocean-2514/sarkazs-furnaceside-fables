using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 043 佣兵：使用后下次战斗中所有能力牌(Power)费用 -1。
public sealed class MercenaryInspirer : ThoughtModel
{
    protected override int InitialRarity => 1;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;

    public override bool CanBeUpgraded => true;

    public override Type UpgradedType => typeof(CivilWar);


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
        if (card.Type != CardType.Power) return false;
        modifiedCost = Math.Max(0, originalCost - 1);
        return true;
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, System.Collections.Generic.IEnumerable<MegaCrit.Sts2.Core.Entities.Creatures.Creature> participants)
    {
        if (side == Owner.Creature.Side && Status == ThoughtStatus.Used) turnLimit--;
        return Task.CompletedTask;
    }
}