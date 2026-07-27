using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 19 萨卡兹枯朽战士：死亡时对我方(玩家)施加 3 层 SkzCorruption；攻击1(2*3) -> 攻击2(8) 循环
public sealed class SarkazWitherer : SarkazMonsterModel
{
    public override int MaxInitialHp => 33;
    public override int MinInitialHp => 33;

    private const int _damage1 = 2;
    private const int _repeat1 = 3;
    private const int _damage2 = 8;

    public override async Task BeforeDeath(Creature creature)
    {
        if (creature != Creature) return;
        foreach (Player player in creature.CombatState.Players)
        {
            await PowerCmd.Apply<SkzCorruptionPower>(new ThrowingPlayerChoiceContext(), player.Creature, 3, null, null);
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new MultiAttackIntent(_damage1, _repeat1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new SingleAttackIntent(_damage2));
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack1;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack1, attack2 }, attack1);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).WithHitCount(_repeat1).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("F_Idle", isLooping: true);
        AnimState attack = new AnimState("F_Attack");
        AnimState die = new AnimState("F_Die");
        attack.NextState = idle;
        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Dead", die);
        return animator;
    }
}
