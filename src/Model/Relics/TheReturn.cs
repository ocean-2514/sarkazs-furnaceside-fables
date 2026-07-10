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

// 095 《归来》：触发闪避后1回合内攻击力+130%
public sealed class TheReturn : RelicModel, ISarkazModel
{
    private bool _active;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public async Task AfterDamageDodged(IRunState runState, ICombatState combatState, Creature target, decimal amount, Creature dealer)
    {
        if (target != Owner.Creature || _active) return;
        _active = true;
        Flash();
        await SarkazRelicFx.ApplySelf<AttackPower>(this, 130);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Creature.Side || !_active) return;
        _active = false;
        await SarkazRelicFx.ApplySelf<AttackPower>(this, -130);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _active = false;
        return Task.CompletedTask;
    }
}
