using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Relics;

// 143 魔王的旗帜：目标生命值达到上限时，攻击速度+30（每回合按条件动态调整）
public sealed class SarkazKingsTornBanner : RelicModel
{
    private decimal _applied;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Creature.Side) return;
        decimal target = Owner.Creature.CurrentHp >= Owner.Creature.MaxHp ? 30 : 0;
        decimal delta = target - _applied;
        if (delta != 0)
        {
            await SarkazRelicFx.ApplySelf<AttackSpeed>(this, delta);
            _applied = target;
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _applied = 0;
        return Task.CompletedTask;
    }
}
