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

// 188 “绽放”：每次受到伤害回复1点能量；每次受到元素（凋亡）损伤额外回复1点能量。
// 凋亡损伤的识别：无来源(dealer==null)且带 Unblockable 的伤害（NecrosisBurst 的施加方式）。
public sealed class Unleashings : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (target != Owner.Creature || result.UnblockedDamage <= 0) return;
        if (!CombatManager.Instance.IsInProgress) return;
        int energy = 1;
        if (dealer == null && props.HasFlag(ValueProp.Unblockable))
        {
            energy += 1;
        }
        Flash();
        await PlayerCmd.GainEnergy(energy, Owner);
    }
    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<UnleashingsPower>(this, 1);
    }
}
