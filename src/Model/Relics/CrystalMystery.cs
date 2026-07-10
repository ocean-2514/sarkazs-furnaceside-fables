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
using Skz.Ocean.Map;
using Skz.Ocean.Model.Acts;
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Model.Relics.Base;

namespace Skz.Ocean.Model.Relics; 

// 159 结晶之谜：每回合获得10点格挡，但第一回合只能打出一张牌（原版 RingingPower：
// 给所有牌上鸣响、打出一张后其余锁定，回合结束自除），仅诡谲断章内生效。所有敌人生命值+10%。
public sealed class CrystalMystery : FragmentMysteryRelic
{
    public override async Task BeforeCombatStart()
    {
        if (!InFragment) return;
        await SarkazRelicFx.ApplyPower<RingingPower>(this, Owner.Creature, 1);
    }

    protected override async Task OnPlayerTurnStartInFragment(ICombatState combatState)
    {
        await CreatureCmd.GainBlock(Owner.Creature, 10, ValueProp.Unpowered, null);
    }
}
