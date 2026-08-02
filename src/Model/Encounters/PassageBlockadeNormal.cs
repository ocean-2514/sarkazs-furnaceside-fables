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

public sealed class PassageBlockadeNormal : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override bool FullyCenterPlayers => true;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<CorruptedKnight>(),
        ModelDb.Monster<WitheredKnight>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<CorruptedKnight>(), "first"),
                new WaveEntry(() => ModelDb.Monster<WitheredKnight>(), "second")
            ]
        ];
    }

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player && combatState.RoundNumber <= 1)
        {
            foreach (Creature creature in combatState.PlayerCreatures)
            {
                await PowerCmd.Apply<SurroundedPower>(new ThrowingPlayerChoiceContext(), creature, 1, null, null);
            }
        }
    }

}
