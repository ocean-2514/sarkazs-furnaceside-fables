using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 8 弑君者：被施加任意层 BlockPower 时立即死亡；攻击1(15) -> 攻击2(30) 循环
public sealed class Crownslayer : SarkazMonsterModel
{
    public override int MaxInitialHp => 80;
    public override int MinInitialHp => 80;

    private const int _damage1 = 15;
    private const int _damage2 = 30;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<CrownslayerPower>(new ThrowingPlayerChoiceContext(), Creature, 1, null, null);
        Creature.PowerApplied += OnPowerApplied;
    }

    public override void BeforeRemovedFromRoom()
    {
        Creature.PowerApplied -= OnPowerApplied;
        base.BeforeRemovedFromRoom();
    }

    // PowerApplied 是同步事件，无法 await：fire-and-forget 一个击杀任务。
    private void OnPowerApplied(PowerModel power)
    {
        if (power is BlockPower)
        {
            TaskHelper.RunSafely(CreatureCmd.TriggerAnim(Creature, "Disappear", 0.2f));
            TaskHelper.RunSafely(CreatureCmd.Kill(Creature));
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new SingleAttackIntent(_damage2));
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack1;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack1, attack2 }, attack1);
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

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("Idle", isLooping: true);
        AnimState attack = new AnimState("Attack");
        AnimState die = new AnimState("Die");
        AnimState disappear = new AnimState("Disappear");
        AnimState appear = new AnimState("Appear");
        attack.NextState = idle;
        disappear.NextState = appear;
        appear.NextState = idle;
        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Disappear", disappear);
        return animator;
    }
}
