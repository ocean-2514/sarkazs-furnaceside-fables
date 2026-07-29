using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 30 大君之触：加入战斗后获得1层SanguinarchPower + 50层DamageReductionPower；护盾(7) -> 攻击(1*7) 循环
public sealed class TouchOfTheSanguinarch : SarkazMonsterModel
{
    public override int MaxInitialHp => 25;
    public override int MinInitialHp => 25;

    private const int _block = 7;
    private const int _damage = 1;
    private const int _repeat = 7;
    private const int _drAmount = 50;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<SanguinarchPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await PowerCmd.Apply<DamageReductionPower>(new ThrowingPlayerChoiceContext(), Creature, _drAmount, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState defend = new MoveState("DEFEND", Defend, new DefendIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new MultiAttackIntent(_damage, _repeat));
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
        await DamageCmd.Attack(_damage).WithHitCount(_repeat).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
