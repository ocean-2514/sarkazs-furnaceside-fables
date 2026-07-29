using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Encounters;

public sealed class CarelessMagicNormal : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<TouchOfTheSanguinarch>(),
        ModelDb.Monster<GiftOfTheSanguinarch>(),
        ModelDb.Monster<SarkazGuerrillaCaster>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<TouchOfTheSanguinarch>(), "first"),
                new WaveEntry(() => ModelDb.Monster<GiftOfTheSanguinarch>(), "second")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<SarkazGuerrillaCaster>(), "second")
            ]
        ];
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player || combatState.RoundNumber > 1) return;
        foreach (Creature creature in combatState.PlayerCreatures)
        {
            await PowerCmd.Apply<OriginiumAltarPower>(new ThrowingPlayerChoiceContext(), creature, 2, null, null);
        }
        foreach (Creature creature1 in combatState.Enemies)
        {
            await PowerCmd.Apply<OriginiumAltarPower>(new ThrowingPlayerChoiceContext(), creature1, 8m, null, null);
        }
    }

    
}
