using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Rewards;

namespace Skz.Ocean.Model.Relics;

// 010 解约协议：护盾+12，攻击力+7%
public sealed class LetterOfTerminationContract : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        Owner.GainShield(12);
        await Task.CompletedTask;
    }

    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<AttackPower>(this, 7);
    }
}
