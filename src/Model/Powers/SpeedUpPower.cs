using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Powers;

public sealed class SpeedUpPower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    private decimal _increasedSpeed = 0;

    public decimal ModifyMoveSpeedAdditive(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster, decimal baseSpeed)
    {
        if (Owner.Monster == null) return 0m;
        if (!(Owner.Monster is SarkazMonsterModel sarkazMonster)) return 0m;
        if (sarkazMonster != monster) return 0m;
        return _increasedSpeed;
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) return Task.CompletedTask;
        _increasedSpeed += 150;
        return Task.CompletedTask;
    }

    public Task AfterMonsterBlocked(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster)
    {
        if (monster != Owner?.Monster) return Task.CompletedTask;
        _increasedSpeed = 0;
        return Task.CompletedTask;
    }
}