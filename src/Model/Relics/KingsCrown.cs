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

// 125 诸王的冠冕：当前生命值≤10或＜最大生命值10%时攻击力+50%，
// 集齐3件及以上国王收藏品（冠冕/新枪/铠甲/延伸）后变为+150%（每回合按条件动态调整）
public sealed class KingsCrown : RelicModel
{
    private decimal _applied;

    public override RelicRarity Rarity => RelicRarity.Rare;

    private bool LowHp => Owner.Creature.CurrentHp <= 10
                          || Owner.Creature.CurrentHp < Owner.Creature.MaxHp * 10 / 100;

    private int KingSetCount => Owner.Relics.Count(r =>
        r is KingsCrown || r is KingsNewLance || r is KingsArmor || r is KingsLegacy);

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Creature.Side) return;
        decimal target = LowHp ? (KingSetCount >= 3 ? 150 : 50) : 0;
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
