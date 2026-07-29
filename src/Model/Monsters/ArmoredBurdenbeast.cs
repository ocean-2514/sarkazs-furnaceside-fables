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

// 28 温顺的武装驮兽：移速150，前两回合不行动，之后 buff(活力3/5/7...递增) -> 攻击(15) 循环
public sealed class ArmoredBurdenbeast : SarkazMonsterModel
{
    public override int MaxInitialHp => 85;
    public override int MinInitialHp => 85;

    public override decimal InitialSpeed => 150;

    private const int _damage = 15;
    private int _vigorStacks = 3;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState idle1 = new MoveState("IDLE1", Idle, new SleepIntent());
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        idle1.FollowUpState = buff;
        buff.FollowUpState = attack;
        attack.FollowUpState = buff;
        return new MonsterMoveStateMachine(new List<MonsterState> { idle1, buff, attack }, idle1);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), Creature, _vigorStacks, Creature, null);
        _vigorStacks += 2;
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private Task Idle(IReadOnlyList<Creature> targets) => Task.CompletedTask;

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("Idle", isLooping: true);
        AnimState attack = new AnimState("Attack");
        AnimState die = new AnimState("Die_1");
        AnimState skillBegin = new AnimState("Skill_Begin");
        AnimState skillLoop = new AnimState("Skill_Loop");
        AnimState skillEnd = new AnimState("Skill_End");
        attack.NextState = idle;
        skillBegin.NextState = skillLoop;
        skillLoop.NextState = skillEnd;
        skillEnd.NextState = idle;
        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Skill", skillBegin);
        return animator;
    }
}
