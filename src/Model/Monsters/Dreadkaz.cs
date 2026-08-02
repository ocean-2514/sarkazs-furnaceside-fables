using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Model.Acts;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Saves;

namespace Skz.Ocean.Model.Monsters;

public sealed class Dreadkaz : SarkazMonsterModel
{
    public override int MaxInitialHp => 30;
    public override int MinInitialHp => 30;

    private int _damage = 10;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();

        RunState state = Traverse.Create(RunManager.Instance).Property("State").GetValue<RunState>();
        int sourceIndex = state.CurrentActIndex;
        if (state?.Act is BizarreFragment)
        {
            sourceIndex = MapDetour.GetSourceAct().Index;
        }

        if (sourceIndex > 0)
        {
            await CreatureCmd.GainMaxHp(Creature, 30 * sourceIndex);
            _damage += 10 * sourceIndex;
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState("ATTACK", Attack, new UnknownIntent());
        attack.FollowUpState = attack;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack }, attack);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
