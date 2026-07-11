using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

public sealed class SpinesOfEpoch : MonsterModel
{
    public override int MaxInitialHp => 30;
    public override int MinInitialHp => 30;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<RewardPlanPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await PowerCmd.Apply<SpinesOfEpochPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = new List<MonsterState>();

        MoveState inflictNecrosis = new MoveState("INFLICT_NECROSIS", InflictNecrosis, new DebuffIntent());
        inflictNecrosis.FollowUpState = inflictNecrosis;
        states.Add(inflictNecrosis);

        return new MonsterMoveStateMachine(states, inflictNecrosis);
    }

    private async Task InflictNecrosis(IReadOnlyList<Creature> targets)
    {
        foreach (Creature target in targets)
        {
            await PowerCmd.Apply<Necrosis>(new ThrowingPlayerChoiceContext(), target, 2, base.Creature, null);
        }
    }
}
