using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Powers;

public sealed class FogMachinePower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    private const decimal _speedMultiplier = 0.4m;

    public decimal ModifyMoveSpeedMultiplicative(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster, decimal baseSpeed)
    {
        if (Owner.Monster == null) return 1m;
        if (!(Owner.Monster is SarkazMonsterModel sarkazMonster)) return 1m;
        if (sarkazMonster != monster) return 1m;
        return _speedMultiplier;
    }

    public override async Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
        {
            await PowerCmd.Decrement(this);
        }
    }

    public override async Task AfterApplied(Creature applier, CardModel cardSource)
    {
        await (Owner?.Monster as SarkazMonsterModel)?.ApplyOrModifyEnterObjectivePower(new ThrowingPlayerChoiceContext());
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        if (oldOwner == null) return;
        await (oldOwner?.Monster as SarkazMonsterModel)?.ApplyOrModifyEnterObjectivePower(new ThrowingPlayerChoiceContext());
    }
}