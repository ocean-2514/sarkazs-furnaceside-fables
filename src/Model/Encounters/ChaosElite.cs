using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Encounters;

public sealed class ChaosElite : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Elite;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<Rotchaser>(),
        ModelDb.Monster<SarkazWitherer>(),
        ModelDb.Monster<SarkazCrossbowman>(),
        ModelDb.Monster<SarkazMazeSpellweaver>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<Rotchaser>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazWitherer>(), "second"),
                new WaveEntry(() => ModelDb.Monster<SarkazCrossbowman>(), "third")
            ],
            // [
            //     new WaveEntry(() => ModelDb.Monster<SarkazMazeSpellweaver>(), "second")
            // ]
        ];
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player || combatState.RoundNumber > 1) return;
        foreach (Creature creature in combatState.PlayerCreatures)
        {
            await PowerCmd.Apply<SkzCorruptionPower>(new ThrowingPlayerChoiceContext(), creature, 3, null, null);
        }
    }



}
