using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
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
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Model;
using Skz.Ocean.Model.Acts;
using Skz.Ocean.Rewards;
using MegaCrit.Sts2.Core.Entities.RestSite;
using Skz.Ocean.Entities.RestSite;
using MegaCrit.Sts2.Core.Logging;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Runs;

public class SarkazManager  //for one player
{
    private readonly Player _player;

    private readonly HpLossVar _fracturedHpLoss = new HpLossVar(1m);
    private readonly HpLossVar _obstructedHpLoss = new HpLossVar(2m);

    public const int PlanCostOnTravel = 2;
    
    public bool CanWalkHorizontally => _player.GetPlanCount() >= PlanCostOnTravel;

    public const float NormalCombatThoughtChance = 0.95f;

    public const float NormalCombatShieldChance = 0.85f;

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

    public async Task AfterRoomEntered(AbstractRoom room)
    {
        _pendingGoldMultiplier = 0;
        _pendingPlanRewards = 0;
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

    public decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props)
    {
        if (target != _player.Creature || props.HasFlag(ValueProp.Unblockable))
        {
            return amount;
        }
        decimal hpLossReduction = Math.Min(amount, _player.GetShield());
        _player.LoseShield((int)hpLossReduction);
        if (hpLossReduction > 0)
        {
            SfxCmd.Play("event:/sfx/block_hit");
        }
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

        rewards.Add(new ThoughtReward(_player, room.RoomType));
        return true;
    }

    public bool TryAddShieldReward(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != _player || room is not CombatRoom) return false;
        bool grant = room.RoomType switch
        {
            RoomType.Monster => _player.PlayerRng.Rewards.NextFloat() < NormalCombatShieldChance,
            RoomType.Elite => true,
            RoomType.Boss => true,
            _ => false
        };
        if (!grant) return false;

        rewards.Add(new ShieldReward(_player, 5));
        return true;
    }

    public bool TryAddInterpretRestSiteOption(Player player, ICollection<RestSiteOption> options)
    {
        if (player != _player) return false;
        options.Add(new InterpretRestSiteOption(player));
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

    // ===== 金币奖励加成 =====
    // 灵感在战斗结束(AfterCombatEnd)时把加成累加到这里(此时已使用思绪还没被清除前调用)，
    // 之后发奖(TryModifyRewards，由 manager 这个常驻监听者触发)时消费。
    private float _pendingGoldMultiplier;

    public void AddGoldMultiplier(float multiplier) => _pendingGoldMultiplier += multiplier;

    // 持久化：AfterCombatEnd(累加) → SaveRun → 发奖(消费)，中间 SL 需要恢复。
    public float PendingGoldMultiplier => _pendingGoldMultiplier;
    public void RestorePendingGoldMultiplier(float value) => _pendingGoldMultiplier = value;

    public bool TryAddGoldBonus(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (player != _player || _pendingGoldMultiplier <= 0f || room is not CombatRoom) return false;

        GoldReward existing = rewards.OfType<GoldReward>().FirstOrDefault();
        float mult = _pendingGoldMultiplier;
        if (existing == null || existing.Amount <= 0) return false;

        int bonus = (int)(existing.Amount * mult);
        if (bonus <= 0) return false;
        rewards.Add(new GoldReward(bonus, player));
        return true;
    }

    // ===== 年代之刺：被玩家击杀后战斗奖励追加构想 =====
    // RewardPlanPower.BeforeDeath 时 ++，发奖(TryModifyRewards)时消费。
    // 战斗结束的 SaveRun 早于奖励生成，所以计数会随侧存档落盘——奖励界面 SL 读档后照发。
    private int _pendingPlanRewards;

    public void RegisterPlanReward() => _pendingPlanRewards++;

    public bool TryAddPlanReward(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        Log.Info($"[sarkaz]: { _pendingPlanRewards }");
        if (player != _player || _pendingPlanRewards <= 0 || room is not CombatRoom) return false;
        for (int i = 0; i < _pendingPlanRewards; i++)
        {
            rewards.Add(new PlanReward(player));
        }
        return true;
    }

    public int PendingPlanRewards => _pendingPlanRewards;
    public void RestorePendingPlanRewards(int count) => _pendingPlanRewards = count;

    // ===== 旧乡晶尘：失去后进入新一层回到手上 =====
    private readonly List<ModelId> _pendingReturns = new List<ModelId>();

    public void RegisterReturn(ModelId id)
    {
        if (!_pendingReturns.Contains(id)) _pendingReturns.Add(id);
    }

    public async Task GrantReturns()
    {
        if (_pendingReturns.Count == 0) return;
        List<ModelId> ids = _pendingReturns.ToList();
        _pendingReturns.Clear();
        foreach (ModelId id in ids)
        {
            ThoughtModel canonical = ModelDb.GetById<ThoughtModel>(id);
            if (canonical != null) await ThoughtCmd.Obtain(canonical.ToMutable(), _player);
        }
    }

    public IReadOnlyList<ModelId> PendingReturns => _pendingReturns;
    public void RestorePendingReturn(ModelId id) => _pendingReturns.Add(id);

    // ===== 诡谲断章：每层地图后半段钉 1(概率 2)个固定事件节点 =====
    // 地图生成后选点记坐标(ModifyGeneratedMapLate)；进点时对该坐标强制事件房型 +
    // 强制 EnterBizarreFragment 事件(LanternKey 同款组合)。坐标持久化到侧存档。
    public const float SecondFragmentChance = 0.25f;

    // actIndex → 该层的固定事件节点坐标（一般 1 个，概率 2 个）
    private readonly Dictionary<int, List<MapCoord>> _fragmentCoords = new Dictionary<int, List<MapCoord>>();

    public bool IsFragmentNode(int actIndex, MapCoord? coord) =>
        coord.HasValue
        && _fragmentCoords.TryGetValue(actIndex, out List<MapCoord> list)
        && list.Contains(coord.Value);

    // 地图生成后调用。同一层只掷一次（读档/回程用快照重建地图时不重掷，坐标仍有效）。
    public void PlaceFragmentNodes(IRunState runState, ActMap map, int actIndex)
    {
        if (runState == null || map == null) return;
        if (actIndex < 0 || actIndex >= runState.Acts.Count) return;
        if (runState.Acts[actIndex] is BizarreFragment) return; // 子层里不再放入口
        if (_fragmentCoords.ContainsKey(actIndex))
        {
            PinFragmentNodes(map, actIndex);
            return;
        }

        Rng rng = (runState as RunState)?.Rng.Niche;
        if (rng == null) return;

        List<MapPoint> points = map.GetAllMapPoints().ToList();
        if (points.Count == 0) return;
        int maxRow = points.Max(p => p.coord.row);

        // 后半段（不会过早出现）的可改 Unknown 节点
        List<MapPoint> candidates = points
            .Where(p => p.PointType == MapPointType.Unknown
                        && p.CanBeModified
                        && p.coord.row >= (maxRow + 1) / 2)
            .ToList();
        if (candidates.Count == 0) return;

        List<MapCoord> chosen = new List<MapCoord>();

        MapPoint first = candidates[rng.NextInt(candidates.Count)];
        chosen.Add(first.coord);
        candidates.Remove(first);

        if (candidates.Count > 0 && rng.NextFloat() < SecondFragmentChance)
        {
            MapPoint second = candidates[rng.NextInt(candidates.Count)];
            chosen.Add(second.coord);
        }

        

        _fragmentCoords[actIndex] = chosen;
        PinFragmentNodes(map, actIndex);
    }

    // 锁住选中的节点：CanBeModified=false 让节点刷新(我们自己的机制)和原版地图修改都跳过它。
    private void PinFragmentNodes(ActMap map, int actIndex)
    {
        if (!_fragmentCoords.TryGetValue(actIndex, out List<MapCoord> coords)) return;
        foreach (MapCoord coord in coords)
        {
            if (map.HasPoint(coord))
            {
                map.GetPoint(coord).CanBeModified = false;
            }
        }
    }

    // 地图(重)生成后给固定节点挂任务图标（SpoilsMap 同款）。MapPoint 每次生成都是新实例，需重挂。
    public void MarkFragmentNodes(ActMap map, int actIndex, AbstractModel questSource)
    {
        if (!_fragmentCoords.TryGetValue(actIndex, out List<MapCoord> coords)) return;
        foreach (MapCoord coord in coords)
        {
            if (map.HasPoint(coord))
            {
                map.GetPoint(coord)?.AddQuest(questSource);
            }
        }
    }

    // 持久化
    public IReadOnlyDictionary<int, List<MapCoord>> FragmentCoords => _fragmentCoords;
    public void RestoreFragmentCoord(int actIndex, MapCoord coord)
    {
        if (!_fragmentCoords.TryGetValue(actIndex, out List<MapCoord> list))
        {
            list = new List<MapCoord>();
            _fragmentCoords[actIndex] = list;
        }
        if (!list.Contains(coord)) list.Add(coord);
    }
}
