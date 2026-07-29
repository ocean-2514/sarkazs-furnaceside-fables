using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 38 萨卡兹悖谬谍报长：debuff(1层DebtPower) -> 攻击1(5 + LoseGold 20) -> 攻击2(3*4) 循环
public sealed class SarkazAbsurdistOperativeLeader : SarkazMonsterModel
{
    public override int MaxInitialHp => 38;
    public override int MinInitialHp => 38;

    private const int _damage1 = 5;
    private const int _damage2 = 3;
    private const int _repeat2 = 4;
    private const int _goldLoss = 20;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1), new DebuffIntent());
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_damage2, _repeat2));
        debuff.FollowUpState = attack1;
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack1;
        return new MonsterMoveStateMachine(new List<MonsterState> { debuff, attack1, attack2 }, debuff);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<DebtPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Attack1", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
        foreach (Creature target in targets)
        {
            if (target.Player != null) await PlayerCmd.LoseGold(_goldLoss, target.Player);
        }
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).WithHitCount(_repeat2).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack2", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("Idle", isLooping: true);
        AnimState attack1 = new AnimState("Attack_1");
        AnimState attack2 = new AnimState("Attack_2");
        AnimState die = new AnimState("Die");
        attack1.NextState = idle;
        attack2.NextState = idle;
        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack1", attack1);
        animator.AddAnyState("Attack2", attack2);
        animator.AddAnyState("Dead", die);
        return animator;
    }
}
