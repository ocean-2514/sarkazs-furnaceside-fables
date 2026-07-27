using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 22 萨卡兹魔剑士：移速300，加入战斗后获得 WaitingPower(不行动)。损失生命值或被激活后苏醒；
// 苏醒后攻击(随机13-23) 循环。
public sealed class SarkazBladeweaver : SarkazMonsterModel
{
    public override int MaxInitialHp => 36;
    public override int MinInitialHp => 36;

    public override decimal InitialSpeed => 300;

    private const int _minDamage = 13;
    private const int _maxDamage = 23;

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

    // 损失生命值即苏醒
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Creature && result.UnblockedDamage > 0)
        {
            await Wake();
        }
    }

    // 供哨兵等外部激活
    public async Task Wake()
    {
        if (IsWaiting) {
            await CreatureCmd.TriggerAnim(Creature, "Wake", 0f);
            await PowerCmd.Remove<WaitingPower>(Creature);
            MoveState moveState = new MoveState("ATTACK", Attack, new SingleAttackIntent(() => _rolledDamage))
            {
                FollowUpStateId = "ATTACK",
                MustPerformOnceBeforeTransitioning = true
            };
            SetMoveImmediate(moveState);
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState sleep = new MoveState("SLEEP", Idle, new SleepIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(() => _rolledDamage));
        ConditionalBranchState branch = new ConditionalBranchState("WAKE_BRANCH");
        branch.AddState(sleep, () => IsWaiting);
        branch.AddState(attack, () => true);
        sleep.FollowUpState = branch;
        attack.FollowUpState = attack;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { sleep, attack, branch }, sleep);
    }

    private Task Idle(IReadOnlyList<Creature> targets) => Task.CompletedTask;

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_rolledDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
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
