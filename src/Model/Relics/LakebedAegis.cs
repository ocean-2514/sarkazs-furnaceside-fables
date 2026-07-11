using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Model.Relics.Base;

namespace Skz.Ocean.Model.Relics;

// 096 湖中神盾：护盾值+4；战斗开始时如果拥有护盾值，本场最大生命+60%
public sealed class LakebedAegis : RelicModel
{
    private int _granted;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        Owner.GainShield(4);
        await Task.CompletedTask;
    }

    public override async Task BeforeCombatStart()
    {
        if (Owner.GetShield() <= 0) return;
        Flash();
        _granted = (int)(Owner.Creature.MaxHp * 60 / 100);
        if (_granted > 0) await CreatureCmd.GainMaxHp(Owner.Creature, _granted);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (_granted > 0 && Owner.Creature.CurrentHp > 0)
        {
            await SarkazRelicFx.LoseMaxHp(Owner.Creature, _granted);
        }
        _granted = 0;
    }
}
