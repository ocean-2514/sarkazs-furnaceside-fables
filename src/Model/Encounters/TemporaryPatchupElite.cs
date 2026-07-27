using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class TemporaryPatchupElite : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Elite;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<SarkazCaster>(),
        ModelDb.Monster<SarkazMazeGuide>(),
        ModelDb.Monster<SarkazMazeReturner>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<SarkazCaster>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazMazeGuide>(), "second")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<SarkazMazeReturner>(), "second")
            ]
        ];
    }
}
