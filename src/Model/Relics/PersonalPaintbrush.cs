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

// 150 随身画笔：构想的负荷-1（现有构想立即生效；此后新获得的构想在 ThoughtCmd.Obtain 里结算）
public sealed class PersonalPaintbrush : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool HasUponPickupEffect => true;

    public override Task AfterObtained()
    {
        foreach (ThoughtModel plan in Owner.GetPlans().ToList())
        {
            plan.Toil = Math.Max(0, plan.Toil - 1);
        }
        return Task.CompletedTask;
    }
}
