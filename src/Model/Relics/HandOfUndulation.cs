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

// 116 波纹之手：每对一个单位造成伤害就获得2点能量并抽一张牌（每个单位每场战斗只触发一次）；
// 拥有断杖-波纹时，攻击速度+20
public sealed class HandOfUndulation : RelicModel
{
    private readonly HashSet<Creature> _hit = new HashSet<Creature>();

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<HandOfUndulationPower>(this, 1);
        if (Owner.Relics.Any(r => r is BrokenWandUndulation))
        {
            await SarkazRelicFx.ApplySelf<AttackSpeed>(this, 20);
        }
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (dealer != Owner.Creature || target == null || !target.IsEnemy) return;
        if (!props.IsPoweredAttack() || _hit.Contains(target)) return;
        _hit.Add(target);
        Flash();
        await PlayerCmd.GainEnergy(2, Owner);
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _hit.Clear();
        return Task.CompletedTask;
    }
}
