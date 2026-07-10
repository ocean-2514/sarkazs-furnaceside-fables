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
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Relics;

// 139 私人巫术终端：进入战斗时，如果使用了灵感，攻击速度+35
public sealed class PersonalWitchcraftTerminal : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task BeforeCombatStart()
    {
        if (Owner.GetThoughtUsed() == null) return;
        Flash();
        await SarkazRelicFx.ApplySelf<AttackSpeed>(this, 35);
    }
}
