using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters.Mocks;

public sealed class MockSpinesOfEpoch : SarkazEncounterModel, ISarkazModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<RadioactiveOriginiumSlug>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<LondiniumMobileDefenseArtillery>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SteamTank>(), "second"),
                new WaveEntry(() => ModelDb.Monster<RemnantOrchestraFlautist>(), "third"),
                new WaveEntry(() => ModelDb.Monster<RemnantOrchestraWindPlayer>(), "forth"),
                new WaveEntry(() => ModelDb.Monster<RemnantOrchestraCellist>(), "fifth"),
            ],
            [
                new WaveEntry(() => ModelDb.Monster<CorruptedKnight>(), "first"),
                new WaveEntry(() => ModelDb.Monster<WitheredKnight>(), "second"),
                new WaveEntry(() => ModelDb.Monster<CausticOriginiumSlug>(), "third"),
                new WaveEntry(() => ModelDb.Monster<SacrilegiousChimera>(), "forth"),
                new WaveEntry(() => ModelDb.Monster<SarkazAbsurdistTyrantCaptain>(), "fifth"),
                new WaveEntry(() => ModelDb.Monster<SeedOfDecay>(), "sixth"),
            ],
            [
                new WaveEntry(() => ModelDb.Monster<DecayingCollectorDisciple>(), "first"),
                new WaveEntry(() => ModelDb.Monster<Skullshatterer>(), "second"),
                new WaveEntry(() => ModelDb.Monster<ImperialArtilleryCoreTargeteer>(), "third"),
            ],
            [
                new WaveEntry(() => ModelDb.Monster<DeathveilAssassin>(), "first"),
                new WaveEntry(() => ModelDb.Monster<OffenderPrototype>(), "second"),
                new WaveEntry(() => ModelDb.Monster<CrossroadsPrototype>(), "third"),
                new WaveEntry(() => ModelDb.Monster<TrafficTowerPrototype>(), "forth"),
                new WaveEntry(() => ModelDb.Monster<TrafficOfficerCaptain>(), "fifth"),
            ],
            
        ];
    }
}