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
using MegaCrit.Sts2.Core.Map;
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
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Model.Relics.Base;
using Skz.Ocean.Model.Thoughts.Inspirers;

namespace Skz.Ocean.Model.Relics;

// 124 金酒之杯：每有15金币，所有我方单位的攻击速度+7（每回合按当前金币动态调整）
public sealed class GoldenChalice : RelicModel
{
    private readonly Dictionary<Creature, decimal> _applied = new Dictionary<Creature, decimal>();

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Creature.Side) return;
        decimal target = Owner.Gold / 30 * 7;
        foreach (Creature ally in combatState.Allies.ToList())
        {
            if (ally == null || !ally.IsAlive) continue;
            decimal current = _applied.TryGetValue(ally, out decimal v) ? v : 0m;
            decimal delta = target - current;
            if (delta != 0)
            {
                await SarkazRelicFx.ApplyPower<AttackSpeed>(this, ally, delta);
                _applied[ally] = target;
            }
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _applied.Clear();
        return Task.CompletedTask;
    }
}
