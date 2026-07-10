using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Model.Relics.Base;

namespace Skz.Ocean.Model.Relics;

// 113 断杖-波纹：场上每有一名敌人，所有敌人获得5%易伤（最多8层；每回合按存活敌人数动态调整）
public sealed class BrokenWandUndulation : RelicModel
{
    private readonly Dictionary<Creature, decimal> _applied = new Dictionary<Creature, decimal>();

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Creature.Side) return;
        List<Creature> alive = combatState.Enemies.Where(e => e != null && e.IsAlive).ToList();
        decimal target = Math.Min(alive.Count, 8) * 5;
        foreach (Creature enemy in alive)
        {
            decimal current = _applied.TryGetValue(enemy, out decimal v) ? v : 0m;
            decimal delta = target - current;
            if (delta != 0)
            {
                await SarkazRelicFx.ApplyPower<InjuredPower>(this, enemy, delta);
                _applied[enemy] = target;
            }
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _applied.Clear();
        return Task.CompletedTask;
    }
}
