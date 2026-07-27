using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 11 萨卡兹术士：第一回合 Debuff(施加1层 RingingPower)，之后 攻击1(7) -> 攻击2(5*2) -> 攻击3(14) 循环
public sealed class SarkazCaster : SarkazMonsterModel
{
    public override int MaxInitialHp => 44;
    public override int MinInitialHp => 44;

    private const int _damage1 = 7;
    private const int _damage2 = 5;
    private const int _repeat2 = 2;
    private const int _damage3 = 14;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_damage2, _repeat2));
        MoveState attack3 = new MoveState("ATTACK3", Attack3, new SingleAttackIntent(_damage3));
        debuff.FollowUpState = attack1;
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack3;
        attack3.FollowUpState = attack1;
        return new MonsterMoveStateMachine(new List<MonsterState> { debuff, attack1, attack2, attack3 }, debuff);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.TriggerAnim(Creature, "Skill", 0f);
        await PowerCmd.Apply<RingingPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).WithHitCount(_repeat2).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack3(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage3).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
