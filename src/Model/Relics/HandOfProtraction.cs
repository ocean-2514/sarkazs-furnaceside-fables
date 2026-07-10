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

// 117 久居之手：每过一个回合攻击力+20%（最多5层，第二个回合开始加）
public sealed class HandOfProtraction : RelicModel
{
    private int _stacks;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Creature.Side || combatState.RoundNumber < 2 || _stacks >= 5) return;
        _stacks++;
        Flash();
        await SarkazRelicFx.ApplySelf<AttackPower>(this, 20);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _stacks = 0;
        return Task.CompletedTask;
    }
    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<HandOfProtractionPower>(this, 1);
    }
}
