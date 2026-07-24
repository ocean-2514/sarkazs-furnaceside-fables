using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Skz.Ocean.Model.Cards;

public sealed class EscapeCraneCard : CardModel, ISarkazModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ CardKeyword.Exhaust ];
    public override int MaxUpgradeLevel => 0;

    public EscapeCraneCard() : 
        base(0, CardType.Skill, CardRarity.Event, TargetType.Self) {}

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int num = Math.Min(1, CardPile.MaxCardsInHand - PileType.Hand.GetPile(base.Owner).Cards.Count);
		if (num > 0)
		{
			await CardPileCmd.Add(await CardSelectCmd.FromCombatPile(choiceContext, PileType.Discard.GetPile(base.Owner), base.Owner, new CardSelectorPrefs(base.SelectionScreenPrompt, num)), PileType.Hand);
		}
    }
}