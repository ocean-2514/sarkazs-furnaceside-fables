using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters.Mocks;

public sealed class MockSpinesOfEpoch : SarkazEncounterModel, ISarkazModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<RadioactiveOriginiumSlug>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<SarkazCrossbowman>(), "second"),
                new WaveEntry(() => ModelDb.Monster<SarkazMazeGuide>(), "first"),
                new WaveEntry(() => ModelDb.Monster<IcefieldArtsFighter>(), "third")
            ]
        ];
    }
}