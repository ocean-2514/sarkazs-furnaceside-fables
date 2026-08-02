using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Encounters;

public sealed class GuiltlessParadiseNormal : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<CausticOriginiumSlug>(),
        ModelDb.Monster<SacrilegiousChimera>(),
        ModelDb.Monster<SarkazWorldcurser>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<CausticOriginiumSlug>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SacrilegiousChimera>(), "second"),
                new WaveEntry(() => ModelDb.Monster<SarkazWorldcurser>(), "third")
            ]
        ];
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player || combatState.RoundNumber > 1) return;
        foreach (Creature creature1 in combatState.Enemies)
        {
            if (creature1.Monster is SarkazWorldcurser)
            {
                await CreatureCmd.GainMaxHp(creature1, 20);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), creature1, 5, null, null);
            }
        }
    }
}
