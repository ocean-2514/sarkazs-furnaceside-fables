using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Cards;

public sealed class RiotPileCard : CardModel, ISarkazModel
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [ CardKeyword.Retain, CardKeyword.Exhaust ];

    public override int MaxUpgradeLevel => 0;

    public RiotPileCard() : 
        base(0, CardType.Skill, CardRarity.Event, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        SarkazMonsterModel sarkazMonster = cardPlay.Target?.Monster as SarkazMonsterModel;
        if (sarkazMonster != null && sarkazMonster.Blockable)
        {
            await PowerCmd.Apply<BlockPower>(choiceContext, cardPlay.Target, 1, Owner.Creature, this);
        }
    }
}