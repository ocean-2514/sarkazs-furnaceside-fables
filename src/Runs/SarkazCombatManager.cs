using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Skz.Ocean.Model;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Runs;

public class SarkazCombatManager
{
    private readonly Player _player;

    private const float SpinesOfEpochChance = 0.6f;

    public SarkazCombatManager(Player player)
    {
        _player = player;
    }

    public async Task BeforeSideTurnStarted(PlayerChoiceContext choiceContext, CombatSide side, ICombatState combatState)
    {
        if (side == CombatSide.Player && combatState.RoundNumber <= 1)
        {
            // await PlayerCmd.GainEnergy(10, _player);
            // await PowerCmd.Apply<StrengthPower>(choiceContext, _player.Creature, 100, null, null);
            if (_player.PlayerRng.Rewards.NextFloat() < SpinesOfEpochChance)
            {
                Creature creature = await CreatureCmd.Add<SpinesOfEpoch>(_player.Creature.CombatState);
                int hpGain = _player.RunState.CurrentActIndex * 30;
                await CreatureCmd.GainMaxHp(creature, hpGain);
            }
        }
    }
}