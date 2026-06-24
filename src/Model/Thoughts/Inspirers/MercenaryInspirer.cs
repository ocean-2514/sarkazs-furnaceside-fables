using System;
using MegaCrit.Sts2.Core.Entities.Cards;
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

    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (Status != ThoughtStatus.Used || card.Owner != Owner) return false;
        if (card.Type != CardType.Power) return false;
        modifiedCost = Math.Max(0, originalCost - 1);
        return true;
    }
}