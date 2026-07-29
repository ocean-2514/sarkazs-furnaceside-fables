using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class SordidSurveillanceElite : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Elite;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<SarkazRoyalCourtSoldier>(),
        ModelDb.Monster<SarkazMazeSpellweaver>(),
        ModelDb.Monster<SarkazMazeGuide>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<SarkazRoyalCourtSoldier>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazMazeSpellweaver>(), "second"),
                new WaveEntry(() => ModelDb.Monster<SarkazMazeGuide>(), "third")
            ]
        ];
    }
}
