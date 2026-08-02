using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace Skz.Ocean.Model.Monsters;

// 33 暴鸰：第一次攻击过后移速*2；攻击(14) -> 不行动
public sealed class Bombtail : SarkazMonsterModel
{
    public override int MaxInitialHp => 28;
    public override int MinInitialHp => 28;

    private const int _damage = 14;
    private bool _attacked;

    public override bool Blockable => false;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        MoveState idle = new MoveState("IDLE", Idle, new UnknownIntent());
        attack.FollowUpState = idle;
        idle.FollowUpState = idle;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack, idle }, attack);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
        _attacked = true;
        BaseSpeedBonus = InitialSpeed;
    }

    private Task Idle(IReadOnlyList<Creature> targets) => Task.CompletedTask;

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("Idle", isLooping: true);
        AnimState idle2 = new AnimState("Idle_2", isLooping: true);
        AnimState attack = new AnimState("Attack");
        AnimState die = new AnimState("Die_2");
        attack.NextState = idle2;
        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Dead", die);
        return animator;
    }
}
