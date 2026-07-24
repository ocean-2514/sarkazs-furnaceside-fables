using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Cards;

public sealed class FogMachineCard : CardModel, ISarkazModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ CardKeyword.Exhaust ];
    public override int MaxUpgradeLevel => 0;

    private const int _duration = 2;

    public FogMachineCard() : 
        base(0, CardType.Skill, CardRarity.Event, TargetType.AnyEnemy) {}

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<FogMachinePower>(choiceContext, cardPlay.Target, _duration, Owner.Creature, this);
    }
}