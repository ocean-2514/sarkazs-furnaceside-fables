using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class BlottedSkyNormal : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<Sniper>(),
        ModelDb.Monster<SeedOfWithering>(),
        ModelDb.Monster<ArmoredBurdenbeast>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<Sniper>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SeedOfWithering>(), "second"),
                new WaveEntry(() => ModelDb.Monster<SeedOfWithering>(), "third")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<ArmoredBurdenbeast>(), "second")
            ]
        ];
    }
}
