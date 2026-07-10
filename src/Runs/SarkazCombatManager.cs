using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Model;
using Skz.Ocean.Model.Acts;
using Skz.Ocean.Model.Monsters;
using Skz.Ocean.Saves;

namespace Skz.Ocean.Runs;

public class SarkazCombatManager
{
    private readonly Player _player;

    private const float SpinesOfEpochChance = 0.6f;

    public SarkazCombatManager(Player player)
    {
        _player = player;
    }

    // ===== 完美作战：本场战斗没有因敌方的攻击而损失生命值 =====
    public bool WasPerfectCombat { get; private set; } = true;

    public void OnCombatStart()
    {
        WasPerfectCombat = true;
    }

    // 敌方攻击造成未被格挡的伤害 → 本场不再是完美作战
    public void OnDamageReceived(Creature target, DamageResult result, Creature dealer)
    {
        if (target != _player.Creature) return;
        if (dealer == null || !dealer.IsEnemy) return;
        if (result.UnblockedDamage <= 0) return;
        WasPerfectCombat = false;
    }

    public async Task BeforeSideTurnStarted(PlayerChoiceContext choiceContext, CombatSide side, ICombatState combatState)
    {
        if (side == CombatSide.Player && combatState.RoundNumber <= 1)
        {
            // 预示之谜(162)：年代之刺生成概率提高（仅诡谲断章内生效）
            float chance = SpinesOfEpochChance;
            bool foresight = _player.RunState.Act is Model.Acts.BizarreFragment
                             && _player.Relics.Any(r => r is Model.Relics.ForesightMystery);
            if (foresight) chance = Math.Min(1f, chance + 0.3f);

            if (_player.PlayerRng.Rewards.NextFloat() < chance)
            {
                Creature creature = await CreatureCmd.Add<SpinesOfEpoch>(_player.Creature.CombatState);
                
                int sourceIndex = _player.RunState.CurrentActIndex;

                RunState state = Traverse.Create(RunManager.Instance).Property("State").GetValue<RunState>();
                if (state?.Act is BizarreFragment)
                {
                    sourceIndex = MapDetour.GetSourceAct().Index;
                }

                int hpGain = sourceIndex * 30;
                await CreatureCmd.GainMaxHp(creature, hpGain);
                // 预示之谜：年代之刺生命值+100%
                if (foresight)
                {
                    await CreatureCmd.GainMaxHp(creature, creature.MaxHp);
                }
                // 雕词錾刀(152)：年代之刺最大生命值下降30%
                if (_player.Relics.Any(r => r is Model.Relics.WordCarvingKnife))
                {
                    await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), creature,
                        creature.MaxHp * 30 / 100, isFromCard: false);
                }
            }
        }
    }
}