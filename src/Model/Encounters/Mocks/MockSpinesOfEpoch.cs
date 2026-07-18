using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters.Mocks;

public sealed class MockSpinesOfEpoch : EncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<SarkazMazeGuide>()
    ];

    protected override IReadOnlyList<(MonsterModel, string)> GenerateMonsters()
    {
        return [
            (ModelDb.Monster<SarkazMazeGuide>().ToMutable(), null)
        ];
    }
}