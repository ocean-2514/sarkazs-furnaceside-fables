using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class DenseJungleElite : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Elite;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<RagingSlugEater>(),
        ModelDb.Monster<SarkazSwordsman>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<RagingSlugEater>(), "first"),
                new WaveEntry(() => ModelDb.Monster<RagingSlugEater>(), "second"),
                new WaveEntry(() => ModelDb.Monster<RagingSlugEater>(), "third"),
                new WaveEntry(() => ModelDb.Monster<RagingSlugEater>(), "fifth"),
            ],
            [
                new WaveEntry(() => ModelDb.Monster<RagingSlugEater>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazSwordsman>(), "second")
            ]
        ];
    }
}
