using System.Collections.Generic;
using System.Threading.Tasks;
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
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace Skz.Ocean.Model.Monsters;

// 44 萨卡兹恶咒者：加入战斗后获得WaitingPower(不动)。损失生命值或被激活后苏醒；
// 苏醒前:不行动；苏醒后:攻击(随机16-25) -> debuff(4层RingingPower) 循环
public sealed class SarkazWorldcurser : SarkazMonsterModel
{
    public override int MaxInitialHp => 55;
    public override int MinInitialHp => 55;

    private const int _minDamage = 16;
    private const int _maxDamage = 25;

    private int _rolledDamage = _minDamage;

    private bool IsWaiting => Creature.HasPower<WaitingPower>();

    private void RollDamage() =>
        _rolledDamage = _minDamage + RunRng.MonsterAi.NextInt(_maxDamage - _minDamage + 1);

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        RollDamage();
        await PowerCmd.Apply<WaitingPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await base.AfterSideTurnEnd(choiceContext, side, participants);
        if (side == Creature.Side) RollDamage();
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Creature && result.UnblockedDamage > 0)
        {
            await Wake();
        }
    }

    public async Task Wake()
    {
        if (!IsWaiting) return;
        await PowerCmd.Remove<WaitingPower>(Creature);
        await CreatureCmd.TriggerAnim(Creature, "Wake", 0f);
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(() => _rolledDamage))
        {
            FollowUpStateId = "DEBUFF",
            MustPerformOnceBeforeTransitioning = true
        };
        SetMoveImmediate(attack, forceTransition: false);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState sleep = new MoveState("SLEEP", Idle, new SleepIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(() => _rolledDamage));
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        ConditionalBranchState branch = new ConditionalBranchState("WAKE_BRANCH");
        branch.AddState(sleep, () => IsWaiting);
        branch.AddState(attack, () => true);
        sleep.FollowUpState = branch;
        attack.FollowUpState = debuff;
        debuff.FollowUpState = attack;
        return new MonsterMoveStateMachine(new List<MonsterState> { sleep, attack, debuff, branch }, sleep);
    }

    private Task Idle(IReadOnlyList<Creature> targets) => Task.CompletedTask;

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_rolledDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        foreach (Creature target in targets)
        {
            if (!target.HasPower<SkzRingingPower>())
            {
                await PowerCmd.Apply<SkzRingingPower>(new ThrowingPlayerChoiceContext(), targets, 4, Creature, null);
            }
        }
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("Idle", isLooping: true);
        AnimState skillIdle = new AnimState("Skill_Idle", isLooping: true);
        AnimState skillAttack = new AnimState("Skill_Attack");
        AnimState skillBegin = new AnimState("Skill_Begin");
        AnimState die = new AnimState("Die");
        skillBegin.NextState = skillIdle;
        skillAttack.NextState = skillIdle;
        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Wake", skillBegin);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Attack", skillAttack);
        return animator;
    }
}
