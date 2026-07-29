using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class TimewornTensionsElite : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Elite;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<SarkazMazeGuide>(),
        ModelDb.Monster<SarkazWorldcurser>(),
        ModelDb.Monster<HolyAutomaton>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<SarkazMazeGuide>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazWorldcurser>(), "second"),
                new WaveEntry(() => ModelDb.Monster<SarkazWorldcurser>(), "third"),
                new WaveEntry(() => ModelDb.Monster<HolyAutomaton>(), "forth")
            ]
        ];
    }
}
