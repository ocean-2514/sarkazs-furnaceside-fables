using MegaCrit.Sts2.Core.Entities.Creatures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Entities.Thoughts;
using MegaCrit.Sts2.Core.Extensions;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 042 驱城：使用后下次战斗的每回合开始时将弃牌堆随机一张牌放入手牌。
public sealed class CityDriving : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (!participants.Contains(base.Owner.Creature))
		{
			return;
		}
		CardPile pile = PileType.Discard.GetPile(base.Owner);
		IEnumerable<CardModel> source = pile.Cards.Where((CardModel c) => c.Type == CardType.Attack || c.Type == CardType.Skill || c.Type == CardType.Power);
		IEnumerable<CardModel> enumerable = source.ToList().UnstableShuffle(base.Owner.RunState.Rng.CombatCardSelection).Take(1);
        foreach (CardModel card in enumerable)
		{
			await CardPileCmd.Add(card, PileType.Hand);
		}
	}
}