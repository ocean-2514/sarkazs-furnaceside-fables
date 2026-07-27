using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class CleartheCrowdElite : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Elite;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<SarkazBladeweaver>(),
        ModelDb.Monster<SarkazSentinel>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<SarkazBladeweaver>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazBladeweaver>(), "second"),
                new WaveEntry(() => ModelDb.Monster<SarkazSentinel>(), "third")
            ]
        ];
    }
}
