using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Map;
using Skz.Ocean.Model.Acts;
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Model.Relics.Base;

namespace Skz.Ocean.Model.Relics;

// 172 戈渎不语：所有敌方单位每回合获得30点格挡，并获得缓慢效果
public sealed class GuldulsSilence : RelicModel
{
    private readonly HashSet<Creature> _slowed = new HashSet<Creature>();

    public override RelicRarity Rarity => RelicRarity.Event;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Owner.Creature == null) return;
        // 缓慢：对新登记的敌人一次性施加（我方回合初检查，覆盖中途入场）
        if (side == Owner.Creature.Side)
        {
            foreach (Creature enemy in combatState.Enemies.ToList())
            {
                if (enemy == null || !enemy.IsAlive || _slowed.Contains(enemy)) continue;
                _slowed.Add(enemy);
                await SarkazRelicFx.ApplyPower<SlowPower>(this, enemy, 1);
            }
        }
        // 每个敌方回合开始：所有存活敌人+30格挡
        else
        {
            foreach (Creature enemy in combatState.Enemies.ToList())
            {
                if (enemy == null || !enemy.IsAlive) continue;
                await CreatureCmd.GainBlock(enemy, 30, ValueProp.Unpowered, null);
            }
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _slowed.Clear();
        return Task.CompletedTask;
    }
}
