using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class HastyEscape : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<HoundPro>(),
        ModelDb.Monster<SarkazCrossbowman>(),
        ModelDb.Monster<Crownslayer>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [ 
                new WaveEntry(() => ModelDb.Monster<HoundPro>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazCrossbowman>(), "second")
            ],
            [ new WaveEntry(() => ModelDb.Monster<Crownslayer>(), "second") ]
        ];
    }
}