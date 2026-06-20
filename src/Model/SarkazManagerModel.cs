using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
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
}
