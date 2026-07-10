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
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Model.Relics.Base;

namespace Skz.Ocean.Model.Relics;

// 018 红色蝴蝶结：护盾+8，每4回合多抽一张牌（全局计数，DisplayAmount 显示进度）
public sealed class RedBowTie : RelicModel
{
    private int _turnCounter;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool HasUponPickupEffect => true;

    public override bool ShowCounter => true;

    public override int DisplayAmount => _turnCounter;

    [SavedProperty]
    public int TurnCounter
    {
        get => _turnCounter;
        set
        {
            AssertMutable();
            _turnCounter = value;
        }
    }

    public override async Task AfterObtained()
    {
        Owner.GainShield(8);
        await Task.CompletedTask;
    }

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner) return count;
        TurnCounter++;
        if (TurnCounter >= 4)
        {
            TurnCounter = 0;
            InvokeDisplayAmountChanged();
            Flash();
            return count + 1;
        }
        InvokeDisplayAmountChanged();
        return count;
    }
}
