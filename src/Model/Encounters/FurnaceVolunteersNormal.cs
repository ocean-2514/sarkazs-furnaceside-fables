using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class FurnaceVolunteersNormal : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<SarkazSwordsman>(),
        ModelDb.Monster<GuerrillaHerald>(),
        ModelDb.Monster<SarkazGuerrillaFighter>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<SarkazSwordsman>(), "first"),
                new WaveEntry(() => ModelDb.Monster<GuerrillaHerald>(), "second")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<SarkazSwordsman>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazGuerrillaFighter>(), "second")
            ]
        ];
    }
}
