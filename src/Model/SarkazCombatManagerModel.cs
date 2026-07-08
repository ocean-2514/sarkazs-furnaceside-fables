using MegaCrit.Sts2.Core.Entities.Creatures;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Runs;

namespace Skz.Ocean.Model;

public class SarkazCombatManagerModel : AbstractModel, ISarkazModel
{
    private SarkazCombatManager _manager = null;

    private Player _owner = null;

    public Player Owner
    {
        get => _owner;
        set
        {
            if (_owner != null && _owner != value)
			{
				throw new InvalidOperationException("Cannot move manager from " + base.Id.Entry + " one owner to another");
			}
			_owner = value;
            _manager = new SarkazCombatManager(_owner);
        }
    } 

    public override bool ShouldReceiveCombatHooks => true;

    public SarkazCombatManagerModel ToMutable()
    {
        AssertCanonical();
        return (SarkazCombatManagerModel)MutableClone();
    }

    public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        await _manager.BeforeSideTurnStarted(choiceContext, side, combatState);
    }
}