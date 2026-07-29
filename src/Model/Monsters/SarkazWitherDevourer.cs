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
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 25 萨卡兹枯朽吞噬者：debuff(1层DevourPower) -> buff(8层覆甲) -> 攻击1(15) -> 攻击2(5*2) 循环
public sealed class SarkazWitherDevourer : SarkazMonsterModel
{
    public override int MaxInitialHp => 60;
    public override int MinInitialHp => 60;

    private const int _damage1 = 15;
    private const int _damage2 = 5;
    private const int _repeat2 = 2;
    private const int _plating = 8;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_damage2, _repeat2));
        debuff.FollowUpState = buff;
        buff.FollowUpState = attack1;
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack1;
        return new MonsterMoveStateMachine(new List<MonsterState> { debuff, buff, attack1, attack2 }, debuff);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<DevourPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), Creature, _plating, Creature, null);
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

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("Idle_B", isLooping: true);
        AnimState attack = new AnimState("Attack_B");
        AnimState die = new AnimState("Die_B");
        attack.NextState = idle;
        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Dead", die);
        return animator;
    }
}
