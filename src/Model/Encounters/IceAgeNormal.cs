using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class IceAgeNormal : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<InfusedGlacialOriginiumSlug>(),
        ModelDb.Monster<IcefieldArtsFighter>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<InfusedGlacialOriginiumSlug>(), "first"),
                new WaveEntry(() => ModelDb.Monster<IcefieldArtsFighter>(), "second")
            ]
        ];
    }
}
