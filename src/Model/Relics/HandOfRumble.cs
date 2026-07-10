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

// 119 轰鸣之手：每对一个单位造成伤害就使自身攻击力+10%，最高+100%，回合结束后清空
public sealed class HandOfRumble : RelicModel
{
    private decimal _applied;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (dealer != Owner.Creature || target == null || !target.IsEnemy) return;
        if (!props.IsPoweredAttack() || _applied >= 100) return;
        _applied += 10;
        await SarkazRelicFx.ApplySelf<AttackPower>(this, 10);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Creature.Side || _applied <= 0) return;
        await SarkazRelicFx.ApplySelf<AttackPower>(this, -_applied);
        _applied = 0;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _applied = 0;
        return Task.CompletedTask;
    }
    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<HandOfRumblePower>(this, 1);
    }
}
