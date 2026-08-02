using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 67 "十字路口"试做型：
// debuff1(1层RestrainPowerCardPower) -> debuff2(1层RestrainSkillCardPower) -> 攻击(28) -> buff(20层AttackPower) 循环
public sealed class CrossroadsPrototype : SarkazMonsterModel
{
    public override int MaxInitialHp => 110;
    public override int MinInitialHp => 110;

    private const int _damage = 28;
    private const int _attackBuff = 20;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState debuff1 = new MoveState("DEBUFF1", Debuff1, new DebuffIntent());
        MoveState debuff2 = new MoveState("DEBUFF2", Debuff2, new DebuffIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        debuff1.FollowUpState = debuff2;
        debuff2.FollowUpState = attack;
        attack.FollowUpState = buff;
        buff.FollowUpState = attack;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { debuff1, debuff2, attack, buff }, debuff1);
    }

    private async Task Debuff1(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<RestrainPowerCardPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }

    private async Task Debuff2(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<RestrainSkillCardPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<AttackPower>(new ThrowingPlayerChoiceContext(), Creature, _attackBuff, Creature, null);
    }
}
