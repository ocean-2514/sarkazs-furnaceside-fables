using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class RegularArmyElite : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Elite;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<ImperialArtilleryCoreTargeteer>(),
        ModelDb.Monster<DeathveilAssassin>(),
        ModelDb.Monster<Skullshatterer>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<ImperialArtilleryCoreTargeteer>(), "first"),
                new WaveEntry(() => ModelDb.Monster<DeathveilAssassin>(), "second")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<Skullshatterer>(), "second")
            ]
        ];
    }
}
