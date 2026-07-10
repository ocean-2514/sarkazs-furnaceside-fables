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

// 106 折戟-浴血：攻击敌人后获得1点能量，每回合仅可触发一次
public sealed class BendSpearsBloodbath : RelicModel
{
    private bool _usedThisTurn;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (_usedThisTurn || dealer != Owner.Creature) return;
        if (target == null || !target.IsEnemy || !props.IsPoweredAttack()) return;
        _usedThisTurn = true;
        Flash();
        await PlayerCmd.GainEnergy(1, Owner);
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Creature.Side) _usedThisTurn = false;
        return Task.CompletedTask;
    }
    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<BendSpearsBloodbathPower>(this, 1);
    }
}
