using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Cards;

public sealed class InterferenceMineCard : CardModel, ISarkazModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ CardKeyword.Exhaust ];

    public override int MaxUpgradeLevel => 0;

    public InterferenceMineCard() : 
        base(0, CardType.Skill, CardRarity.Event, TargetType.AnyEnemy) {}

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<InterferenceMinePower>(choiceContext, cardPlay.Target, 1, Owner.Creature, this);
    }
}