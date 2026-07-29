using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class FirepowerSquadNormal : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<Bombtail>(),
        ModelDb.Monster<AntiArmorInfantry>(),
        ModelDb.Monster<WarPhantom>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<Bombtail>(), "first"),
                new WaveEntry(() => ModelDb.Monster<AntiArmorInfantry>(), "second")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<WarPhantom>(), "second")
            ]
        ];
    }
}
