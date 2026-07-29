using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class BloodlineDebatesNormal : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<InfusedOriginiumSlug>(),
        ModelDb.Monster<SarkazWitherDevourer>(),
        ModelDb.Monster<SarkazMazeGuide>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<InfusedOriginiumSlug>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazWitherDevourer>(), "second")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<SarkazMazeGuide>(), "second")
            ]
        ];
    }
}
