using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 46 深池逐火战士：死亡后不离场，回5血+99层无实体。1回合内没打死就复活。
// 正常：攻击1(7) -> 攻击2(11) -> 攻击3(3*3) 循环
//
// 实现：ModifyHpLostAfterOsty 在致命伤害生效前钳制到剩 1 HP
// （否则 LoseHpInternal 直接设 WasTargetKilled=true → AfterDamageReceived 根本不触发）。
// 后续 AfterDamageReceived 里进 deadState，一回合后 revive 回正常循环。
// 假死期间再被打死 = 真死。
public sealed class DublinnFlamechaserSoldier : SarkazMonsterModel
{
    public override int MaxInitialHp => 41;
    public override int MinInitialHp => 41;

    private const int _damage1 = 7;
    private const int _damage2 = 11;
    private const int _damage3 = 3;
    private const int _repeat3 = 3;

    private MoveState _deadState;
    private bool _inDeadState;
    private bool _shouldEnterDeadState;

    // 致命伤害生效前钳制，留 1 HP 保 AfterDamageReceived 能被调到
    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (_inDeadState || target != Creature || amount <= 0) return amount;
        if (target.CurrentHp - amount <= 0)
        {
            _shouldEnterDeadState = true;
            return target.CurrentHp - 1; // 留 1 HP
        }
        return amount;
    }

    // 被 ModifyHpLost 救下来后在这里进假死
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!_shouldEnterDeadState || target != Creature) return;
        _shouldEnterDeadState = false;
        _inDeadState = true;
        await CreatureCmd.TriggerAnim(Creature, "Die1", 0f);
        // HP 现在是 1，补 4 = 5
        if (Creature.CurrentHp > 0) await CreatureCmd.Heal(Creature, 4);
        await PowerCmd.Apply<IntangiblePower>(new ThrowingPlayerChoiceContext(), Creature, 99, Creature, null);
        SetMoveImmediate(_deadState, forceTransition: true);
    }

    // 复活：清无实体，回到正常 attack1
    private async Task Revive(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.TriggerAnim(Creature, "Revive", 0f);
        await CreatureCmd.Heal(Creature, Creature.MaxHp);
        if (Creature.HasPower<IntangiblePower>())
            await PowerCmd.Remove<IntangiblePower>(Creature);
        _inDeadState = false;
    }

    private Task DeadIdle(IReadOnlyList<Creature> targets) => Task.CompletedTask;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new SingleAttackIntent(_damage2));
        MoveState attack3 = new MoveState("ATTACK3", Attack3, new MultiAttackIntent(_damage3, _repeat3));
        _deadState = new MoveState("DEAD", DeadIdle)
        {
            MustPerformOnceBeforeTransitioning = true
        };
        MoveState revive = new MoveState("REVIVE", Revive, new HealIntent())
        {
            MustPerformOnceBeforeTransitioning = true
        };
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack3;
        attack3.FollowUpState = attack1;
        _deadState.FollowUpState = revive;
        revive.FollowUpState = attack1;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { attack1, attack2, attack3, _deadState, revive }, attack1);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack3(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage3).WithHitCount(_repeat3).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("Idle", isLooping: true);
        AnimState idle2 = new AnimState("Idle_2", isLooping: true);
        AnimState attack = new AnimState("Attack");
        AnimState die = new AnimState("Die");
        AnimState die2 = new AnimState("Die_2");
        AnimState reviveAnim = new AnimState("Revive");
        attack.NextState = idle;
        reviveAnim.NextState = idle;
        die.NextState = idle2;
        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Dead", die2);
        animator.AddAnyState("Die1", die);
        animator.AddAnyState("Revive", reviveAnim);
        return animator;
    }
}
