using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 26 狙击步兵：攻击1(6) -> buff(力量+2) -> 攻击2(1*4 + 每人3张Dazed加入抽牌堆) 循环
public sealed class Sniper : SarkazMonsterModel
{
    public override int MaxInitialHp => 38;
    public override int MinInitialHp => 38;

    private const int _damage1 = 6;
    private const int _damage2 = 1;
    private const int _repeat2 = 4;
    private const int _dazedPerTarget = 3;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_damage2, _repeat2), new CardDebuffIntent());
        attack1.FollowUpState = buff;
        buff.FollowUpState = attack2;
        attack2.FollowUpState = attack1;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack1, buff, attack2 }, attack1);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).WithHitCount(_repeat2).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Dazed>(targets, PileType.Draw, _dazedPerTarget, null);
    }
}
