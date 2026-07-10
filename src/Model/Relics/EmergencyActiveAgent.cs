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

// 103 紧急活性剂：生命值越低攻击速度越快，30%生命值时最大+60（每回合开始按当前生命调整）
public sealed class EmergencyActiveAgent : RelicModel
{
    private decimal _applied;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Creature.Side) return;
        decimal ratio = Owner.Creature.MaxHp > 0 ? Owner.Creature.CurrentHp / Owner.Creature.MaxHp : 1m;
        decimal t = Math.Clamp((1m - ratio) / 0.7m, 0m, 1m);
        decimal targetAmount = Math.Round(60m * t);
        decimal delta = targetAmount - _applied;
        if (delta != 0)
        {
            await SarkazRelicFx.ApplySelf<AttackSpeed>(this, delta);
            _applied = targetAmount;
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _applied = 0;
        return Task.CompletedTask;
    }
}
