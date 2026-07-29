using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Powers;

public sealed class SanguinarchPower : PowerModel, ISarkazModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterApplied(Creature applier, CardModel cardSource)
    {
        foreach (Creature creature in Owner.CombatState.PlayerCreatures)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), creature, -1, null, null);
            await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), creature, -1, null, null);
        }
    }

    public override async Task BeforeDeath(Creature creature)
    {
        if (creature != Owner) return;
        foreach (Creature target in creature.CombatState.PlayerCreatures)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, 1, null, null);
            await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), target, 1, null, null);
        }
    }

    public async Task BeforeMonsterEscape(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster)
    {
        if (monster != Owner.Monster) return;
        foreach (Creature target in monster.CombatState.PlayerCreatures)
        {
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, 1, null, null);
            await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), target, 1, null, null);
        }
    }
    
}
