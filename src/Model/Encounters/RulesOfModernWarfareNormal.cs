using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class RulesOfModernWarfareNormal : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<RhineDefenseMember>(),
        ModelDb.Monster<RhineEngineeringMember>(),
        ModelDb.Monster<R31aHeavyPowerArmor>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<RhineDefenseMember>(), "second")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<RhineEngineeringMember>(), "first"),
                new WaveEntry(() => ModelDb.Monster<R31aHeavyPowerArmor>(), "second")
            ]
        ];
    }
}
