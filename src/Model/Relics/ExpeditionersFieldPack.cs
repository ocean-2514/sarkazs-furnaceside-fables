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

// 130 探索者背包：被敌方施加负面状态后，攻击力+25%，持续1回合
public sealed class ExpeditionersFieldPack : RelicModel
{
    private bool _active;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature applier, CardModel cardSource)
    {
        if (_active || power == null || power.Owner != Owner.Creature) return;
        if (power.Type != PowerType.Debuff || amount <= 0) return;
        if (applier == null || !applier.IsEnemy) return;
        _active = true;
        Flash();
        await SarkazRelicFx.ApplySelf<AttackPower>(this, 25);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Creature.Side || !_active) return;
        _active = false;
        await SarkazRelicFx.ApplySelf<AttackPower>(this, -25);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _active = false;
        return Task.CompletedTask;
    }
}
