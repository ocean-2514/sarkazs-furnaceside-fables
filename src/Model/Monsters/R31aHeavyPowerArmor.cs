using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 43 R-31a重型动力装甲：加入战斗后获得1层RhineArmorPower；
// 不行动 -> 攻击1(6*5) -> debuff(20层InjuredPower) -> 攻击2(随机10-20) 循环
public sealed class R31aHeavyPowerArmor : SarkazMonsterModel
{
    public override int MaxInitialHp => 90;
    public override int MinInitialHp => 90;

    private const int _damage1 = 6;
    private const int _repeat1 = 5;
    private const int _minDamage2 = 10;
    private const int _maxDamage2 = 20;

    private int _rolledDamage2 = _minDamage2;

    private void RollDamage() =>
        _rolledDamage2 = _minDamage2 + RunRng.MonsterAi.NextInt(_maxDamage2 - _minDamage2 + 1);

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        RollDamage();
        await PowerCmd.Apply<RhineArmorPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await base.AfterSideTurnEnd(choiceContext, side, participants);
        if (side == Creature.Side) RollDamage();
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState idle = new MoveState("IDLE", Idle, new UnknownIntent());
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new MultiAttackIntent(_damage1, _repeat1));
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new SingleAttackIntent(() => _rolledDamage2));
        idle.FollowUpState = attack1;
        attack1.FollowUpState = debuff;
        debuff.FollowUpState = attack2;
        attack2.FollowUpState = attack2;
        return new MonsterMoveStateMachine(new List<MonsterState> { idle, attack1, debuff, attack2 }, idle);
    }

    private Task Idle(IReadOnlyList<Creature> targets) => Task.CompletedTask;

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).WithHitCount(_repeat1).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Skill", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<InjuredPower>(new ThrowingPlayerChoiceContext(), targets, 20, Creature, null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_rolledDamage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("Idle", isLooping: true);
        AnimState attack = new AnimState("Attack");
        AnimState die = new AnimState("Die_02");
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
