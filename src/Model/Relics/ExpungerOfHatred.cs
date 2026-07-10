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

// 191 死仇葬送者：思维负荷（负荷临界点）+5；作战中如果使用了死斗，完美作战后将再次获得死斗
public sealed class ExpungerOfHatred : RelicModel
{
    private bool _usedDeathmatch;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        Owner.IncreaseToilLimit(5);
        await Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        _usedDeathmatch = Owner.GetThoughtUsed() is Deathmatch;
        return Task.CompletedTask;
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (!_usedDeathmatch) return;
        _usedDeathmatch = false;
        if (Owner.GetCombatManager()?.WasPerfectCombat != true) return;
        Flash();
        await ThoughtCmd.Obtain<Deathmatch>(Owner);
    }
}
