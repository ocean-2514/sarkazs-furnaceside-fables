using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
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
        return _manager.ModifyHpLostAfterOstyLate(target, amount);
    }

    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        return _manager.TryAddThoughtReward(player, rewards, room);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _manager.AfterCombatEnd(room);
        return Task.CompletedTask;
    }

    // 进入新一层（新地图）时清空节点刷新次数。
    public override Task AfterActEntered()
    {
        _manager?.ClearRefreshCounts();
        return Task.CompletedTask;
    }

    // ===== 节点刷新：供 NNormalMapPoint 补丁调用 =====
    public int MaxRefreshCount => _manager?.MaxRefreshCount ?? 0;
    public bool CanRefreshNode(MapPoint point) => _manager?.CanRefresh(point) ?? false;
    public bool RefreshNode(MapPoint point) => _manager?.Refresh(point) ?? false;

    // 持久化
    public IReadOnlyDictionary<MapCoord, int> RefreshCounts =>
        _manager?.RefreshCounts ?? new Dictionary<MapCoord, int>();
    public void RestoreRefreshCount(MapCoord coord, int count) => _manager?.RestoreRefreshCount(coord, count);
}
