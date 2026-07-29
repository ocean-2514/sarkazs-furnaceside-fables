using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 2 磐蟹：Buff(获得10点格挡) -> 攻击(7) 循环
public sealed class MetalCrab : SarkazMonsterModel
{
    public override int MaxInitialHp => 24;
    public override int MinInitialHp => 24;

    private const int _block = 10;
    private const int _damage = 7;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState defend = new MoveState("DEFEND", Defend, new DefendIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        defend.FollowUpState = attack;
        attack.FollowUpState = defend;
        return new MonsterMoveStateMachine(new List<MonsterState> { defend, attack }, defend);
    }

    private async Task Defend(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, _block, ValueProp.Move, null);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
