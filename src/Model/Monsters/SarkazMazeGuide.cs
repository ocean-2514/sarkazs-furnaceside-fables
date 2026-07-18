using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;

namespace Skz.Ocean.Model.Monsters;

public sealed class SarkazMazeGuide : MonsterModel
{
    public override int MaxInitialHp => 30;

    public override int MinInitialHp => 30;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = new List<MonsterState>();

        MoveState moveState = new MoveState("attack", Attack, new SingleAttackIntent(10));
        moveState.FollowUpState = moveState;

        states.Add(moveState);

        return new MonsterMoveStateMachine(states, moveState);
    }

    private async Task Attack(IReadOnlyList<Creature> creatures)
    {
        await DamageCmd.Attack(10).FromMonster(this).Execute(null);
    }
}