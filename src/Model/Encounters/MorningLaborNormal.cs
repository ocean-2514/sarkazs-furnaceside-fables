using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class MorningLaborNormal : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<Lurker>(),
        ModelDb.Monster<SarkazLancer>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<Lurker>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazLancer>(), "second")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<Lurker>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazLancer>(), "second")
            ]
        ];
    }
}
