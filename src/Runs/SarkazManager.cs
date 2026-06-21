using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Rewards;

namespace Skz.Ocean.Runs;

public class SarkazManager  //for one player
{
    private readonly Player _player;

    private readonly HpLossVar _fracturedHpLoss = new HpLossVar(1m);
    private readonly HpLossVar _obstructedHpLoss = new HpLossVar(2m);

    public const int PlanCostOnTravel = 2;
    
    public bool CanWalkHorizontally => _player.GetPlanCount() >= PlanCostOnTravel;

    public static int MaxNecrosisAmount => 6;

    public const float NormalCombatThoughtChance = 0.6f;

    // 节点刷新可刷出的类型池（暂等权）。
    private static readonly MapPointType[] _refreshPool =
    {
        MapPointType.Shop, MapPointType.Treasure, MapPointType.RestSite,
        MapPointType.Monster, MapPointType.Elite, MapPointType.Unknown,
    };

    public int MaxRefreshCount { get; set; } = 1;

    // 当前这张地图各节点已刷新次数，按坐标存（MapPoint 实例读档会重建，坐标才稳定）。
    private readonly Dictionary<MapCoord, int> _refreshCounts = new Dictionary<MapCoord, int>();

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

    public void AfterCombatEnd(CombatRoom room)
    {
        _player.IncreaseToilLimit(room.RoomType switch
        {
            RoomType.Monster => 1,
            RoomType.Elite => 2,
            RoomType.Boss => 3,
            _ => 0
        });
    }

    public bool TryAddThoughtReward(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != _player || room is not CombatRoom) return false;

        Rng rng = _player.PlayerRng.Rewards;
        bool grant = room.RoomType switch
        {
            RoomType.Elite => true,
            RoomType.Boss => true,
            RoomType.Monster => rng.NextFloat() < NormalCombatThoughtChance,
            _ => false,
        };
        if (!grant) return false;

        rewards.Add(new ThoughtReward(_player));
        return true;
    }

    // ===== 节点刷新 =====

    public int GetRefreshCount(MapCoord coord) =>
        _refreshCounts.TryGetValue(coord, out int c) ? c : 0;

    // 非 UI 条件：节点可改 + 还有刷新次数 + 至少 1 个构想。
    public bool CanRefresh(MapPoint point) =>
        point != null
        && point.CanBeModified
        && GetRefreshCount(point.coord) < MaxRefreshCount
        && _player.GetPlanCount() >= 1;

    // 刷新一个节点：换成不同于当前的随机类型，计数 +1，扣 1 构想。成功返回 true。
    public bool Refresh(MapPoint point)
    {
        if (!CanRefresh(point)) return false;

        Rng rng = (_player.RunState as RunState)?.Rng.Niche;
        point.PointType = PickNewType(point.PointType, rng);
        _refreshCounts[point.coord] = GetRefreshCount(point.coord) + 1;
        _player.RemovePlans(1); // 会触发 PlayerThoughtChanged → 所有刷新按钮刷新可见性
        return true;
    }

    private static MapPointType PickNewType(MapPointType current, Rng rng)
    {
        List<MapPointType> candidates = new List<MapPointType>(_refreshPool.Length);
        foreach (MapPointType t in _refreshPool)
        {
            if (t != current) candidates.Add(t);
        }
        if (candidates.Count == 0) return current;
        return candidates[rng?.NextInt(candidates.Count) ?? 0];
    }

    // 换层(新地图)时清空——坐标会复用，旧计数必须清掉。
    public void ClearRefreshCounts() => _refreshCounts.Clear();

    // 持久化用：导出/恢复刷新次数（节点类型本身由官方地图存档自动持久化，这里只存次数）。
    public IReadOnlyDictionary<MapCoord, int> RefreshCounts => _refreshCounts;
    public void RestoreRefreshCount(MapCoord coord, int count) => _refreshCounts[coord] = count;
}
