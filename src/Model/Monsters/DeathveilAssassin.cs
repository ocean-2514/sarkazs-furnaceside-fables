using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 65 裹骸死士：加入战斗后获得1层AssassinPower；
// 攻击1(18) -> buff(20层PenetratePower) -> 之后攻击2(随机12-18 *2) -> 攻击1 循环
public sealed class DeathveilAssassin : SarkazMonsterModel
{
    public override int MaxInitialHp => 110;
    public override int MinInitialHp => 110;

    private const int _damage1 = 18;
    private const int _penetrate = 20;
    private const int _minDamage2 = 12;
    private const int _maxDamage2 = 18;
    private const int _repeat2 = 2;

    private int _rolledDamage2 = _minDamage2;

    private void RollDamage() =>
        _rolledDamage2 = _minDamage2 + RunRng.MonsterAi.NextInt(_maxDamage2 - _minDamage2 + 1);

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        RollDamage();
        await PowerCmd.Apply<AssassinPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await base.AfterSideTurnEnd(choiceContext, side, participants);
        if (side == Creature.Side) RollDamage();
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1First = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_minDamage2, _repeat2));
        MoveState attack1Loop = new MoveState("ATTACK1_LOOP", Attack1, new SingleAttackIntent(_damage1));
        attack1First.FollowUpState = buff;
        buff.FollowUpState = attack2;
        attack2.FollowUpState = attack1Loop;
        attack1Loop.FollowUpState = attack2;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { attack1First, buff, attack2, attack1Loop }, attack1First);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<PenetratePower>(new ThrowingPlayerChoiceContext(), Creature, _penetrate, Creature, null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_rolledDamage2).WithHitCount(_repeat2).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
