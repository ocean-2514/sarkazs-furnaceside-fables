using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class VerminWeak : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<MetalCrab>(),
        ModelDb.Monster<OriginiumSlugBeta>(),
        ModelDb.Monster<RadioactiveOriginiumSlug>()
    ];

    public override bool IsWeak => true;

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<RadioactiveOriginiumSlug>(), "first"),
                new WaveEntry(() => ModelDb.Monster<OriginiumSlugBeta>(), "second")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<MetalCrab>(), "second")
            ]
        ];
    }
}