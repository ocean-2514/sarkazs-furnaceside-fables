using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts.Interpreter;
using Skz.Ocean.Factories;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Relics;

// 184 烟花之手：攻击牌的每段伤害有25%概率额外对所有敌人造成
// 相当于自身 AttackPower 层数10%的群体伤害（如攻击力+30% → 3点）
public sealed class HandOfFireworks : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (dealer != Owner.Creature || target == null || !target.IsEnemy) return;
        if (!props.IsPoweredAttack() || cardSource == null || cardSource.Type != CardType.Attack) return;
        decimal attackAmount = Owner.Creature.Powers
            .FirstOrDefault(p => p is AttackPower)?.Amount ?? 0m;
        decimal aoe = attackAmount * 10 / 100;
        if (aoe <= 0) return;
        Rng rng = (Owner.RunState as RunState)?.Rng.Niche;
        if (rng == null || rng.NextFloat() >= 0.25f) return;
        Flash();
        foreach (Creature enemy in Owner.Creature.CombatState.Enemies.ToList())
        {
            if (enemy == null || !enemy.IsAlive) continue;
            await CreatureCmd.Damage(choiceContext, enemy, aoe,
                ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
        }
    }
    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<HandOfFireworksPower>(this, 1);
    }
}
