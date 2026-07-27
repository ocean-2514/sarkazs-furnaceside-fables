using System.Collections.Generic;
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

// 15 弧光突击队：第一回合攻击(7，无 Soar) -> 第二回合 Buff(获得 SOAR_POWER) -> 之后攻击(6*2，有 Soar) 循环
public sealed class ArcCommando : SarkazMonsterModel
{
    public override int MaxInitialHp => 28;
    public override int MinInitialHp => 28;

    private const int _plainDamage = 7;
    private const int _soarDamage = 6;
    private const int _soarHits = 2;

    private bool _isHovering = false;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attackPlain = new MoveState("ATTACK_PLAIN", AttackPlain, new SingleAttackIntent(_plainDamage));
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState attackSoar = new MoveState("ATTACK_SOAR", AttackSoar, new MultiAttackIntent(_soarDamage, _soarHits));
        attackPlain.FollowUpState = buff;
        buff.FollowUpState = attackSoar;
        attackSoar.FollowUpState = attackSoar;
        return new MonsterMoveStateMachine(new List<MonsterState> { attackPlain, buff, attackSoar }, attackPlain);
    }

    private async Task AttackPlain(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_plainDamage).FromMonster(this).WithAttackerAnim("Attack1", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.TriggerAnim(Creature, "Takeoff", 0f);
        await PowerCmd.Apply<SoarPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        _isHovering = true;
    }

    private async Task AttackSoar(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_soarDamage).WithHitCount(_soarHits).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack2", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle1 = new AnimState("Idle_1", isLooping: true);
        AnimState idle2 = new AnimState("Idle_2", isLooping: true);
        AnimState attack1 = new AnimState("Attack_1");
        AnimState attack2 = new AnimState("Attack_2");
        AnimState die = new AnimState("Die_2");
        AnimState takeoff = new AnimState("Takeoff");
        attack1.NextState = idle1;
        attack2.NextState = idle2;
        takeoff.NextState = idle2;
        CreatureAnimator animator = new CreatureAnimator(idle1, controller);
        animator.AddAnyState("Attack1", attack1);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Attack2", attack2);
        animator.AddAnyState("Takeoff", takeoff);
        return animator;
    }
}
