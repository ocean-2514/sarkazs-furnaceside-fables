using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Model.Relics.Base;
using Skz.Ocean.Model.Thoughts.Inspirers;

namespace Skz.Ocean.Model.Relics;

// 137 国王的铠甲：护盾值+12，将当前生命值全部转化为4倍护盾值（转化后生命值为1）；
// 战斗结束后使目标生命-4（不致命），护盾值+4
public sealed class KingsArmor : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        decimal converted = Owner.Creature.CurrentHp - 1;
        Owner.GainShield(12 + (int)(converted * 4));
        if (converted > 0)
        {
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner.Creature, converted,
                ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        }
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        decimal loss = Math.Min(4m, Owner.Creature.CurrentHp - 1);
        if (loss > 0)
        {
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner.Creature, loss,
                ValueProp.Unblockable | ValueProp.Unpowered, null, null);
        }
        Owner.GainShield(4);
    }
}
