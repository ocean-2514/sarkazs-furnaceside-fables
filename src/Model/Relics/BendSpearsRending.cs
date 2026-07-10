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

// 112 折戟-裂岩：场上每有一名敌人，攻击力+5%（最多8层；每回合按存活敌人数动态调整）
public sealed class BendSpearsRending : RelicModel
{
    private decimal _applied;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Creature.Side) return;
        int enemies = combatState.Enemies.Count(e => e != null && e.IsAlive);
        decimal target = Math.Min(enemies, 8) * 5;
        decimal delta = target - _applied;
        if (delta != 0)
        {
            await SarkazRelicFx.ApplySelf<AttackPower>(this, delta);
            _applied = target;
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _applied = 0;
        return Task.CompletedTask;
    }
}
