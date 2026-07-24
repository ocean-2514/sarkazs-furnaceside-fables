using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

public sealed class SarkazMazeGuide : SarkazMonsterModel
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
        await DamageCmd.Attack(10).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .Execute(null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState animState = new AnimState("Idle", true);
        AnimState animState1 = new AnimState("Attack");
        CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
        creatureAnimator.AddAnyState("Attack", animState1);
        animState1.NextState = animState;
        return creatureAnimator;
    }
}