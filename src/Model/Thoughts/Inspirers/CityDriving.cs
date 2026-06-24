using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 042 驱城：使用后下次战斗的每回合结束时将所有牌放回抽牌堆。
public sealed class CityDriving : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (Status != ThoughtStatus.Used || side != Owner.Creature.Side) return;
        List<CardModel> cards = new List<CardModel>();
        cards.AddRange(PileType.Hand.GetPile(Owner).Cards);
        cards.AddRange(PileType.Discard.GetPile(Owner).Cards);
        if (cards.Count > 0) await CardPileCmd.Add(cards, PileType.Draw);
    }
}