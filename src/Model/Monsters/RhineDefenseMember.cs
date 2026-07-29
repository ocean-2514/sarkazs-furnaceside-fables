using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 41 莱茵生命防卫科成员：加入战斗后获得1层BufferPower；攻击(9) -> 护盾(7) 循环
public sealed class RhineDefenseMember : SarkazMonsterModel
{
    public override int MaxInitialHp => 31;
    public override int MinInitialHp => 31;

    private const int _damage = 9;
    private const int _block = 7;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<BufferPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        MoveState defend = new MoveState("DEFEND", Defend, new DefendIntent());
        attack.FollowUpState = defend;
        defend.FollowUpState = attack;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack, defend }, attack);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Defend(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, _block, ValueProp.Move, null);
    }
}
