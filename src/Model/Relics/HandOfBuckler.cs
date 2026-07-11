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

// 114 扣挠之手：攻击时额外造成目标当前生命值3%的真实伤害，最少为2
public sealed class HandOfBuckler : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (dealer != Owner.Creature || target == null || !target.IsEnemy || !target.IsAlive) return;
        // 只对"有威力加成的攻击"触发；追加的真实伤害是 Unpowered，不会递归触发自身
        if (!props.IsPoweredAttack()) return;
        decimal extra = Math.Max(2m, target.CurrentHp * 3 / 100);
        await CreatureCmd.Damage(choiceContext, target, extra,
            ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
    }
    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<HandOfBucklerPower>(this, 1);
    }
}
