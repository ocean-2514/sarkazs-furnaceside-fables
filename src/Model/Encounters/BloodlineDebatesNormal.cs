using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class BloodlineDebatesNormal : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<InfusedOriginiumSlug>(),
        ModelDb.Monster<SarkazWitherDevourer>(),
        ModelDb.Monster<SarkazMazeGuide>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<InfusedOriginiumSlug>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazWitherDevourer>(), "second")
            ],
            [
                new WaveEntry(() => ModelDb.Monster<SarkazMazeGuide>(), "second")
            ]
        ];
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player && combatState.RoundNumber <= 1)
        {
            foreach (Creature creature in combatState.Enemies)
            {
                if (creature.Monster is SarkazMazeGuide)
                {
                    await CreatureCmd.GainMaxHp(creature, 20);
                    await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), creature, 5, null, null);
                }
            }
        }
    }
}
