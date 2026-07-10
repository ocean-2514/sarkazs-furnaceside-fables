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

// 167 “小方块”：基础生命值+10%，在Boss战中额外+80%（临时，战后移除）
public sealed class LittleCube : RelicModel
{
    private int _granted;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        await SarkazRelicFx.GainBaseHpPercent(Owner, 10);
    }

    public override async Task BeforeCombatStart()
    {
        if (Owner.RunState.CurrentRoom?.RoomType != RoomType.Boss) return;
        Flash();
        _granted = Owner.Character.StartingHp * 80 / 100;
        if (_granted > 0) await CreatureCmd.GainMaxHp(Owner.Creature, _granted);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (_granted > 0 && Owner.Creature.CurrentHp > 0)
        {
            await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner.Creature, _granted, isFromCard: false);
        }
        _granted = 0;
    }
}
