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

// 022 沉眠同胞：护盾+20，3场战斗后失去该遗物并获得一件珍贵(Rare)遗物
public sealed class HibernatingKin : RelicModel
{
    private int _combats;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool HasUponPickupEffect => true;

    public override bool ShowCounter => true;

    public override int DisplayAmount => _combats;

    [SavedProperty]
    public int Combats
    {
        get => _combats;
        set
        {
            AssertMutable();
            _combats = value;
        }
    }

    public override async Task AfterObtained()
    {
        Owner.GainShield(20);
        await Task.CompletedTask;
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        Combats++;
        InvokeDisplayAmountChanged();
        if (Combats < 3) return;
        RelicModel next = RelicFactory.PullNextRelicFromFront(Owner, RelicRarity.Rare);
        if (next == null) return;
        Flash();
        await RelicCmd.Replace(this, next.ToMutable());
    }
}
