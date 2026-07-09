using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Map;
using Skz.Ocean.Model.Acts;
using Skz.Ocean.Runs;

namespace Skz.Ocean.Model;

public class SarkazManagerModel : AbstractModel, ISarkazModel
{
    public override bool ShouldReceiveCombatHooks => true;

    private SarkazManager _manager = null;

    private Player _owner;

    public bool CanWalkHorizontally => _manager?.CanWalkHorizontally ?? false;

    public Player Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			if (_owner != null && _owner != value)
			{
				throw new InvalidOperationException("Cannot move manager from " + base.Id.Entry + " one owner to another");
			}
			_owner = value;
            _manager = new SarkazManager(_owner);
		}
	}

    public SarkazManagerModel ToMutable()
    {
        AssertCanonical();
        return (SarkazManagerModel)MutableClone();
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        await _manager.AfterRoomEntered(room);
    }

    public override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature dealer, CardModel cardSource)
    {
        return _manager.ModifyHpLostAfterOstyLate(target, amount, props);
    }

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        bool flag1 = _manager.TryAddThoughtReward(player, rewards, room);
        bool flag2 = _manager.TryAddShieldReward(player, rewards, room);
        bool flag3 = _manager.TryAddGoldBonus(player, rewards, room);
        bool flag4 = _manager.TryAddPlanReward(player, rewards, room);
        return flag1 || flag2 || flag3 || flag4;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _manager.AfterCombatEnd(room);
        return Task.CompletedTask;
    }

    public override async Task AfterActEntered()
    {
        _manager?.ClearRefreshCounts();
        if (_manager != null) await _manager.GrantReturns();
    }

    public override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
    {
        return _manager?.TryAddInterpretRestSiteOption(player, options) ?? false;
    }

    // ===== 金币加成 / 待返还思绪：供思绪调用 =====
    public void AddGoldMultiplier(float multiplier) => _manager?.AddGoldMultiplier(multiplier);
    public float PendingGoldMultiplier => _manager?.PendingGoldMultiplier ?? 0f;
    public void RestorePendingGoldMultiplier(float value) => _manager?.RestorePendingGoldMultiplier(value);
    public void RegisterReturn(ModelId id) => _manager?.RegisterReturn(id);
    public IReadOnlyList<ModelId> PendingReturns => _manager?.PendingReturns ?? new List<ModelId>();
    public void RestorePendingReturn(ModelId id) => _manager?.RestorePendingReturn(id);

    // ===== 年代之刺构想奖励：供 RewardPlanPower / 侧存档调用 =====
    public void RegisterPlanReward() => _manager?.RegisterPlanReward();
    public int PendingPlanRewards => _manager?.PendingPlanRewards ?? 0;
    public void RestorePendingPlanRewards(int count) => _manager?.RestorePendingPlanRewards(count);

    // ===== 节点刷新：供 NNormalMapPoint 补丁调用 =====
    public int MaxRefreshCount => _manager?.MaxRefreshCount ?? 0;
    public bool CanRefreshNode(MapPoint point) => _manager?.CanRefresh(point) ?? false;
    public bool RefreshNode(MapPoint point) => _manager?.Refresh(point) ?? false;

    // 持久化
    public IReadOnlyDictionary<MapCoord, int> RefreshCounts =>
        _manager?.RefreshCounts ?? new Dictionary<MapCoord, int>();
    public void RestoreRefreshCount(MapCoord coord, int count) => _manager?.RestoreRefreshCount(coord, count);

    // ===== 诡谲断章：每层地图钉 1(概率 2)个固定事件节点 =====
    // 子层用固定结构地图（GoldenCompass 同款 hook；只在全新生成时触发，
    // 读档/回程走 SavedActMap 分支不经这里，随机布局天然持久化）。
    public override ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
    {
        if (actIndex >= 0 && actIndex < runState.Acts.Count && runState.Acts[actIndex] is BizarreFragment)
        {
            return new BizarreFragmentActMap(runState);
        }
        return map;
    }

    // 生成后选点（读档/回程重建地图时不重掷）。
    public override ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
    {
        _manager?.PlaceFragmentNodes(runState, map, actIndex);
        return map;
    }

    // 每次地图(重)生成后给固定节点挂任务图标（MapPoint 是新实例，需重挂）。
    public override Task AfterMapGenerated(ActMap map, int actIndex)
    {
        _manager?.MarkFragmentNodes(map, actIndex, this);
        return Task.CompletedTask;
    }

    // 走上固定节点(Unknown)时强制事件房型 + 强制进入事件（LanternKey 同款组合）。
    // 子层内则对所有 Unknown（只有出口节点）强制事件 + ExitBizarreFragment
    // （子层 RoomSet 事件池被 GenerateRooms 混入了共享事件，必须在这里钉死）。
    public override IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(IReadOnlySet<RoomType> roomTypes)
    {
        if (IsOnFragmentNode() || InFragmentAct())
        {
            return new HashSet<RoomType> { RoomType.Event };
        }
        return roomTypes;
    }

    public override EventModel ModifyNextEvent(EventModel currentEvent)
    {
        if (InFragmentAct())
        {
            return ModelDb.Event<Events.ExitBizarreFragment>();
        }
        if (IsOnFragmentNode())
        {
            return ModelDb.Event<Events.EnterBizarreFragment>();
        }
        return currentEvent;
    }

    private bool IsOnFragmentNode()
    {
        if (_manager == null || _owner?.RunState == null) return false;
        return _manager.IsFragmentNode(_owner.RunState.CurrentActIndex, _owner.RunState.CurrentMapCoord);
    }

    private bool InFragmentAct()
    {
        return _owner?.RunState?.Act is BizarreFragment;
    }

    // 持久化
    public IReadOnlyDictionary<int, List<MapCoord>> FragmentCoords =>
        _manager?.FragmentCoords ?? new Dictionary<int, List<MapCoord>>();
    public void RestoreFragmentCoord(int actIndex, MapCoord coord) => _manager?.RestoreFragmentCoord(actIndex, coord);
}
