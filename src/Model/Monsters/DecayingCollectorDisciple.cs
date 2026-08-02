using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 62 枯朽萃聚使徒：不可阻挡；不行动 -> 召唤(3个SeedOfDecay) 循环(每2回合一次)
public sealed class DecayingCollectorDisciple : SarkazMonsterModel
{
    public override int MaxInitialHp => 108;
    public override int MinInitialHp => 108;

    public override bool Blockable => false;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState idle = new MoveState("IDLE", Idle, new UnknownIntent());
        MoveState summon = new MoveState("SUMMON", Summon, new SummonIntent());
        idle.FollowUpState = summon;
        summon.FollowUpState = idle;
        return new MonsterMoveStateMachine(new List<MonsterState> { idle, summon }, summon);
    }

    private Task Idle(IReadOnlyList<Creature> targets) => Task.CompletedTask;

    private async Task Summon(IReadOnlyList<Creature> targets)
    {
        string[] slots = { "forth", "fifth", "sixth" };
        for (int i = 0; i < 3; i++)
        {
            await CreatureCmd.Add(ModelDb.Monster<SeedOfDecay>().ToMutable(), CombatState,
                CombatSide.Enemy, slots[i]);
        }
    }
}
