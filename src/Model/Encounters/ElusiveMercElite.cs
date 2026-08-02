using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class ElusiveMercElite : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Elite;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<DublinnFlamechaserSoldier>(),
        ModelDb.Monster<SarkazMazeSpellweaver>(),
        ModelDb.Monster<W>(),
        ModelDb.Monster<Frost>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<DublinnFlamechaserSoldier>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazMazeSpellweaver>(), "second"),
                new WaveEntry(() => ModelDb.Monster<Frost>(), "third")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<W>(), "first"),
                new WaveEntry(() => ModelDb.Monster<Frost>(), "second")
            ]
        ];
    }
}
