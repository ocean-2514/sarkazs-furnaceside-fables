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

// 108 铁卫-无锋：受到攻击后获得1点能量，每回合仅可触发一次。
// 受伤发生在敌方回合时，能量无法保留到己方回合（换回合时被重置），
// 因此将能量发放延迟到己方回合开始(AfterEnergyReset)后发放。
public sealed class IronGuardImpenetrable : RelicModel
{
    private bool _triggered;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (_triggered || target != Owner.Creature) return;
        if (dealer == null || !dealer.IsEnemy || !props.IsPoweredAttack()) return;
        _triggered = true;
        Flash();
    }

    public override async Task AfterEnergyReset(Player player)
    {
        if (!_triggered || player != Owner) return;
        _triggered = false;
        await PlayerCmd.GainEnergy(1, Owner);
    }
    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<IronGuardImpenetrablePower>(this, 1);
    }
}
