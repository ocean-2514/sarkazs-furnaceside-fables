using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 47 W：加入战斗后获得50层HardenedShellPower；
// buff(20 AttackPower) -> 重击(25) -> debuff(易伤1) -> 攻击(15) 循环。
// 生命值首次≤50%时立即重击(SetMoveImmediate)。
public sealed class W : SarkazMonsterModel
{
    public override int MaxInitialHp => 100;
    public override int MinInitialHp => 100;

    private const int _shell = 50;
    private const int _attackBuff = 20;
    private const int _heavyDamage = 25;
    private const int _damage = 15;

    private bool _enraged;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<HardenedShellPower>(new ThrowingPlayerChoiceContext(), Creature, _shell, Creature, null);
    }

    // 生命值首次≤50%：立即重击
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (_enraged || target != Creature || Creature.IsDead) return;
        if ((decimal)Creature.CurrentHp > (decimal)Creature.MaxHp / 2m) return;
        _enraged = true;
        MoveState heavy = new MoveState("HEAVY", Heavy, new SingleAttackIntent(_heavyDamage))
        {
            FollowUpStateId = "DEBUFF",
            MustPerformOnceBeforeTransitioning = true
        };
        SetMoveImmediate(heavy, forceTransition: true);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState heavy = new MoveState("HEAVY", Heavy, new SingleAttackIntent(_heavyDamage));
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        buff.FollowUpState = heavy;
        heavy.FollowUpState = debuff;
        debuff.FollowUpState = attack;
        attack.FollowUpState = buff;
        return new MonsterMoveStateMachine(new List<MonsterState> { buff, heavy, debuff, attack }, buff);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<AttackPower>(new ThrowingPlayerChoiceContext(), Creature, _attackBuff, Creature, null);
    }

    private async Task Heavy(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_heavyDamage).FromMonster(this).WithAttackerAnim("Skill", 0f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("Idle", isLooping: true);
        AnimState attack = new AnimState("Attack");
        AnimState die = new AnimState("Die");
        AnimState skill = new AnimState("Skill");
        skill.NextState = idle;
        attack.NextState = idle;
        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Skill", skill);
        return animator;
    }
}
