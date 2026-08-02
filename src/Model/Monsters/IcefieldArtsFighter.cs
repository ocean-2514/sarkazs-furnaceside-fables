using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 10 冰原卫法者：加入战斗后获得 10 层 IceBurstBeta；攻击(11) -> Debuff(施加1层寒冷) -> Debuff(使一张牌变为消耗) 循环
public sealed class IcefieldArtsFighter : SarkazMonsterModel
{
    public override int MaxInitialHp => 42;
    public override int MinInitialHp => 42;

    private const int _damage = 14;

    private int _exhaustedCardCount = 3;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<IceBurstBeta>(new ThrowingPlayerChoiceContext(), Creature, 10, null, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        MoveState debuff1 = new MoveState("DEBUFF1", Debuff1, new DebuffIntent());
        MoveState debuff2 = new MoveState("DEBUFF2", Debuff2, new CardDebuffIntent());
        attack.FollowUpState = debuff1;
        debuff1.FollowUpState = debuff2;
        debuff2.FollowUpState = attack;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack, debuff1, debuff2 }, attack);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Debuff1(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<ColdPower>(new ThrowingPlayerChoiceContext(), targets, 2, Creature, null);
    }
    
    private async Task Debuff2(IReadOnlyList<Creature> targets)
    {
        foreach (Player player in Creature.CombatState.Players)
        {
            IEnumerable<CardModel> allCards = player.PlayerCombatState.AllCards;
            IEnumerable<CardModel> candidates = allCards.Where(card => !card.Keywords.Contains(CardKeyword.Exhaust));
            for (int i = 0; i < _exhaustedCardCount; i++)
            {
                CardModel card = RunRng.MonsterAi.NextItem(candidates);
                if (card != null)
                {
                    card.AddKeyword(CardKeyword.Exhaust);
                }
                candidates = candidates.Where(card => !card.Keywords.Contains(CardKeyword.Exhaust));
            }
        }
    }
}
