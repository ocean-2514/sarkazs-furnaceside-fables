using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 13 萨卡兹迷宫归乡者：年代之刺在场时攻击变为 2 连击；
// Buff(力量+2) -> 攻击1(3) -> 攻击2(5 + 脆弱1) -> 攻击3(7) 循环
public sealed class SarkazMazeReturner : SarkazMonsterModel
{
    public override int MaxInitialHp => 48;
    public override int MinInitialHp => 48;

    private const int _damage1 = 3;
    private const int _damage2 = 5;
    private const int _damage3 = 7;

    private bool SpinesPresent =>
        Creature.CombatState.Enemies.Any(c => c.IsAlive && c.Monster is SpinesOfEpoch);

    private int Hits => SpinesPresent ? 2 : 1;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new MultiAttackIntent(_damage1, () => Hits));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_damage2, () => Hits), new DebuffIntent());
        MoveState attack3 = new MoveState("ATTACK3", Attack3, new MultiAttackIntent(_damage3, () => Hits));
        buff.FollowUpState = attack1;
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack3;
        attack3.FollowUpState = buff;
        return new MonsterMoveStateMachine(new List<MonsterState> { buff, attack1, attack2, attack3 }, buff);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).WithHitCount(Hits).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim(Hits > 1 ? "Skill" : "Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).WithHitCount(Hits).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim(Hits > 1 ? "Skill" : "Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }

    private async Task Attack3(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage3).WithHitCount(Hits).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim(Hits > 1 ? "Skill" : "Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("Idle", isLooping: true);
        AnimState attack = new AnimState("Attack");
        AnimState die = new AnimState("Die");
        AnimState skill = new AnimState("Skill");
        attack.NextState = idle;
        skill.NextState = idle;
        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Skill", skill);
        return animator;
    }
}
