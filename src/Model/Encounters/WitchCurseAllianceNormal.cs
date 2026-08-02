using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class WitchCurseAllianceNormal : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<RemnantOrchestraFlautist>(),
        ModelDb.Monster<RemnantOrchestraWindPlayer>(),
        ModelDb.Monster<RemnantOrchestraCellist>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<RemnantOrchestraFlautist>(), "first"),
                new WaveEntry(() => ModelDb.Monster<RemnantOrchestraWindPlayer>(), "second")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<RemnantOrchestraCellist>(), "second")
            ]
        ];
    }
}
