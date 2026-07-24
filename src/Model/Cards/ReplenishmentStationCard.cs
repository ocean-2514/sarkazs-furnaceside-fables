using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Skz.Ocean.Model.Cards;

public sealed class ReplenishmentStationCard : CardModel, ISarkazModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ CardKeyword.Exhaust ];

    public override int MaxUpgradeLevel => 0;

    public ReplenishmentStationCard() : 
        base(0, CardType.Skill, CardRarity.Event, TargetType.Self) {}

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(1m, Owner);
    }
}