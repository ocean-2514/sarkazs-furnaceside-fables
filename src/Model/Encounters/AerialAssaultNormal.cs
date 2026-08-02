using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class AerialAssaultNormal : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<SarkazAbsurdistTyrantCaptain>(),
        ModelDb.Monster<SeedOfDecay>(),
        ModelDb.Monster<DecayingCollectorDisciple>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<SarkazAbsurdistTyrantCaptain>(), "second")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<SeedOfDecay>(), "first"),
                new WaveEntry(() => ModelDb.Monster<DecayingCollectorDisciple>(), "second"),
                new WaveEntry(() => ModelDb.Monster<SeedOfDecay>(), "third")
            ]
        ];
    }
}
