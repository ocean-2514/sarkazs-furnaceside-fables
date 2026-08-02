using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 36 铁砧：前两回合治疗(所有敌方回5血)，随后逃离
public sealed class Anvil : SarkazMonsterModel
{
    public override int MaxInitialHp => 20;
    public override int MinInitialHp => 20;

    private const int _heal = 8;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, null, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState heal1 = new MoveState("HEAL1", Heal, new HealIntent());
        MoveState heal2 = new MoveState("HEAL2", Heal, new HealIntent());
        MoveState escape = new MoveState("ESCAPE", Escape, new EscapeIntent());
        heal1.FollowUpState = heal2;
        heal2.FollowUpState = escape;
        escape.FollowUpState = escape;
        return new MonsterMoveStateMachine(new List<MonsterState> { heal1, heal2, escape }, heal1);
    }

    private async Task Heal(IReadOnlyList<Creature> targets)
    {
        foreach (Creature enemy in Creature.CombatState.Enemies)
        {
            if (enemy.IsAlive) await CreatureCmd.Heal(enemy, _heal);
        }
    }

    private async Task Escape(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.Escape(Creature);
    }
}
