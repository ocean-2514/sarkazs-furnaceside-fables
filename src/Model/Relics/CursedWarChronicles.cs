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

// 042 遭诅战史集：负荷临界点+2；每次完美作战后，负荷临界点+1
public sealed class CursedWarChronicles : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        Owner.IncreaseToilLimit(2);
        await Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (Owner.GetCombatManager()?.WasPerfectCombat == true)
        {
            Flash();
            Owner.IncreaseToilLimit(1);
        }
        return Task.CompletedTask;
    }
}
