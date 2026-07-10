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

// 129 Friston.P：每通过一场精英作战，攻击速度永久+5（最多叠加15层）
public sealed class FristonP : RelicModel
{
    private int _stacks;

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool ShowCounter => true;

    public override int DisplayAmount => _stacks;

    [SavedProperty]
    public int Stacks
    {
        get => _stacks;
        set
        {
            AssertMutable();
            _stacks = value;
        }
    }

    public override async Task BeforeCombatStart()
    {
        if (Stacks > 0) await SarkazRelicFx.ApplySelf<AttackSpeed>(this, Stacks * 5);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        if (room.RoomType == RoomType.Elite && Stacks < 15)
        {
            Stacks++;
            InvokeDisplayAmountChanged();
            Flash();
        }
        return Task.CompletedTask;
    }
}
