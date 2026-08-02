using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters;

public sealed class SordidSurveillanceElite : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<SarkazRoyalCourtSoldier>(),
        ModelDb.Monster<SarkazMazeSpellweaver>(),
        ModelDb.Monster<SarkazMazeGuide>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<SarkazRoyalCourtSoldier>(), "first"),
                new WaveEntry(() => ModelDb.Monster<SarkazMazeSpellweaver>(), "second"),
                new WaveEntry(() => ModelDb.Monster<SarkazMazeGuide>(), "third")
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
