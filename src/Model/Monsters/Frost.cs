using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 48 寒霜：在场时我方(玩家)攻速-50；状态机：不攻击
public sealed class Frost : SarkazMonsterModel
{
    public override int MaxInitialHp => 30;
    public override int MinInitialHp => 30;

    private const int _speedDebuff = 30;

    public override bool Blockable => false;


    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        foreach (Creature playerCreature in Creature.CombatState.PlayerCreatures)
        {
            await PowerCmd.Apply<AttackSpeed>(new ThrowingPlayerChoiceContext(), playerCreature, -_speedDebuff, Creature, null);
        }
    }

    public override async Task BeforeDeath(Creature creature)
    {
        if (creature != Creature) return;
        await RemoveAura(creature.CombatState);
    }

    // ISarkazModel：Escape 前收回光环（基类 AfterSideTurnEnd → SarkazHook → 这里）
    public override async Task BeforeMonsterEscape(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster)
    {
        // Log.Info("[sarkaz] frost escaped");
        if (monster != this) return;
        await RemoveAura(combatState);
    }


    private async Task RemoveAura(ICombatState combatState)
    {
        Log.Info("[sarkaz] remove aura!!");
        foreach (Creature playerCreature in combatState.PlayerCreatures)
        {
            if (!playerCreature.IsAlive) continue;
            await PowerCmd.Apply<AttackSpeed>(new ThrowingPlayerChoiceContext(), playerCreature, _speedDebuff, Creature, null);
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
