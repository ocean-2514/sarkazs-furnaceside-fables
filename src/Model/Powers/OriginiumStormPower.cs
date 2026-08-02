using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Skz.Ocean.Model.Powers;

public sealed class OriginiumStormPower : PowerModel
{
	public override PowerType Type => PowerType.Debuff;

	public override PowerStackType StackType => PowerStackType.Counter;

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (participants.Contains(base.Owner))
		{
			foreach (Creature creature in Owner.CombatState.PlayerCreatures)
			{
				await CreatureCmd.Damage(choiceContext, creature, base.Amount, ValueProp.Unpowered | ValueProp.Unblockable, null, null);
			}
			foreach (Creature creature1 in Owner.CombatState.Enemies)
			{
				await CreatureCmd.Damage(choiceContext, creature1, base.Amount, ValueProp.Unpowered | ValueProp.Unblockable, null, null);
			}
		}
	}
}
