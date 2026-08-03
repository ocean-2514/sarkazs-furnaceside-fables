using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Model;
using Skz.Ocean.Model.Acts;
using Skz.Ocean.Model.Cards;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Saves;

namespace Skz.Ocean.Runs;

public class SarkazCombatManager
{
    private readonly Player _player;

    private float SpinesOfEpochChance => AscensionHelper.GetValueIfAscension(AscensionLevel.Scarcity, 0.65f, 0.6f);

    public int BlockDeviceNum => AscensionHelper.GetValueIfAscension(AscensionLevel.WearyTraveler, 0, 1);

    private int _act2Buff = 10;
    private int _act3Buff = 40;

    private HashSet<Creature> _ascensionBuffedEnemies = new HashSet<Creature>();

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
        int sourceIndex = _player.RunState.CurrentActIndex;
        RunState state = Traverse.Create(RunManager.Instance).Property("State").GetValue<RunState>();
        if (state?.Act is BizarreFragment)
        {
            sourceIndex = MapDetour.GetSourceAct().Index;
        }

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

            if (combatState.Encounter is SarkazEncounterModel)
            {
                //防暴桩
                for (int i = 0; i <= Math.Min(sourceIndex, BlockDeviceNum); ++i)
                {
                    await CardPileCmd.AddGeneratedCardToCombat(combatState.CreateCard<RiotPileCard>(_player), PileType.Hand, null);
                }
            }

            foreach (Creature enemy in combatState.Enemies)
            {
                await ApplyAscensionBuff(enemy, sourceIndex);
            }
        }
    }

    public async Task AfterCreatureAddedToCombat(Creature creature)
    {
        if (creature.Side == CombatSide.Player) return;
        int sourceIndex = _player.RunState.CurrentActIndex;
        RunState state = Traverse.Create(RunManager.Instance).Property("State").GetValue<RunState>();
        if (state?.Act is BizarreFragment)
        {
            sourceIndex = MapDetour.GetSourceAct().Index;
        }

        await ApplyAscensionBuff(creature, sourceIndex);
    }

    private async Task ApplyAscensionBuff(Creature enemy, int sourceActIndex)
    {
        if (!RunManager.Instance.HasAscension(AscensionLevel.DoubleBoss)) return;
        if (enemy == null || !enemy.IsAlive || _ascensionBuffedEnemies.Contains(enemy)) return;
        _ascensionBuffedEnemies.Add(enemy);
        if (sourceActIndex == 1)
        {
            await PowerCmd.Apply<AttackSpeed>(new ThrowingPlayerChoiceContext(), enemy, _act2Buff, null, null);
            await PowerCmd.Apply<DamageReductionPower>(new ThrowingPlayerChoiceContext(), enemy, _act2Buff, null, null);
        }
        else if (sourceActIndex == 2)
        {
            await PowerCmd.Apply<AttackSpeed>(new ThrowingPlayerChoiceContext(), enemy, _act3Buff, null, null);
            await PowerCmd.Apply<DamageReductionPower>(new ThrowingPlayerChoiceContext(), enemy, _act3Buff, null, null);
        }
    }
}