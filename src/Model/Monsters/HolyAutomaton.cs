using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Hooks;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 45 圣卫自动机：移速500，不攻击。Escape 后对全场(敌我双方)造成10点真实伤害。
public sealed class HolyAutomaton : SarkazMonsterModel
{
    public override int MaxInitialHp => 30;
    public override int MinInitialHp => 30;

    public override decimal InitialSpeed => 500;

    public override decimal EscapeDamage => 0m; // 不用基类默认的5点可格挡

    // Escape 伤害改在 AfterSideTurnEnd 里手控（走完位移后对全场放真伤再 Escape）
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Creature.Side) return;
        if (Creature.IsDead || Creature.CombatState?.IsLiveCombat() != true) return;

        ICombatState combatState = Creature.CombatState;
        decimal delta = SarkazHook.ModifyMonsterMoveDelta(combatState.RunState, combatState, this, CurrentSpeed);
        CurrentLocation = _currentLocation + Math.Max(0, delta);
        await ApplyOrModifyEnterObjectivePower(choiceContext);

        if (_currentLocation >= Destination)
        {
            // 全场敌我双方10点真实伤害
            foreach (Creature c in combatState.Enemies.Concat(combatState.Players.Select(p => p.Creature)))
            {
                if (c == null || c.IsDead) continue;
                await CreatureCmd.Damage(choiceContext, c, 10m,
                    ValueProp.Unblockable | ValueProp.Unpowered, Creature);
            }
            await CreatureCmd.Escape(Creature);
            await SarkazHook.AfterMonsterEscaped(combatState.RunState, combatState, this);
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState idle = new MoveState("IDLE", Idle, new UnknownIntent());
        idle.FollowUpState = idle;
        return new MonsterMoveStateMachine(new List<MonsterState> { idle }, idle);
    }

    private Task Idle(IReadOnlyList<Creature> targets) => Task.CompletedTask;
}
