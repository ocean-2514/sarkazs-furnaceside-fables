using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Entities.Players;

namespace Skz.Ocean.Runs;

public class SarkazManager  //for one player
{
    private readonly Player _player;

    private readonly HpLossVar _fracturedHpLoss = new HpLossVar(1m);
    private readonly HpLossVar _obstructedHpLoss = new HpLossVar(2m);

    public const int PlanCostOnTravel = 2;
    
    public bool CanWalkHorizontally => _player.GetPlanCount() >= PlanCostOnTravel;

    public SarkazManager(Player player)
    {
        _player = player;
    }

    public async Task AfterRoomEntered()
    {
        await HandleToil();
        HandleHorizontalTravelCost();
    }

    private async Task HandleToil()
    {
        if (_player.Creature.CurrentHp <= 10) return;
        if (_player.GetMindState() == MindState.Fractured)
        {
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), _player.Creature, _fracturedHpLoss.BaseValue,
                ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        }
        else if (_player.GetMindState() == MindState.Obstructed)
        {
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), _player.Creature, _obstructedHpLoss.BaseValue,
                ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        }
    }

    private void HandleHorizontalTravelCost()
    {
        if (!(_player.RunState is RunState runState)) return;
        IReadOnlyList<MapCoord> coords = runState.VisitedMapCoords;
        if (coords.Count <= 1) return;
        if (runState.CurrentRoomCount > 1) return; // 只在首次进入该节点时结算一次
        MapCoord prev = coords[coords.Count - 2];
        MapCoord cur = coords[coords.Count - 1];
        if (prev.row != cur.row) return;          
        _player.RemovePlans(PlanCostOnTravel);
    }

    public decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount)
    {
        if (target != _player.Creature)
        {
            return amount;
        }
        decimal hpLossReduction = Math.Min(amount, _player.GetShield());
        _player.LoseShield((int)hpLossReduction);
        return Math.Max(0, amount - hpLossReduction);
    }
}
