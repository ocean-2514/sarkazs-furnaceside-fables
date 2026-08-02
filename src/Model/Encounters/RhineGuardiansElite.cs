using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class RhineGuardiansElite : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Elite;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<CrossroadsPrototype>(),
        ModelDb.Monster<OffenderPrototype>(),
        ModelDb.Monster<TrafficTowerPrototype>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<CrossroadsPrototype>(), "first"),
                new WaveEntry(() => ModelDb.Monster<OffenderPrototype>(), "second")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<TrafficTowerPrototype>(), "second")
            ]
        ];
    }
}
