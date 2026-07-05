using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Entities.Players;
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
        await _manager.AfterRoomEntered();
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
        return flag1 || flag2 || flag3;
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
    public void RegisterReturn(ModelId id) => _manager?.RegisterReturn(id);
    public IReadOnlyList<ModelId> PendingReturns => _manager?.PendingReturns ?? new List<ModelId>();
    public void RestorePendingReturn(ModelId id) => _manager?.RestorePendingReturn(id);

    // ===== 节点刷新：供 NNormalMapPoint 补丁调用 =====
    public int MaxRefreshCount => _manager?.MaxRefreshCount ?? 0;
    public bool CanRefreshNode(MapPoint point) => _manager?.CanRefresh(point) ?? false;
    public bool RefreshNode(MapPoint point) => _manager?.Refresh(point) ?? false;

    // 持久化
    public IReadOnlyDictionary<MapCoord, int> RefreshCounts =>
        _manager?.RefreshCounts ?? new Dictionary<MapCoord, int>();
    public void RestoreRefreshCount(MapCoord coord, int count) => _manager?.RestoreRefreshCount(coord, count);
}
