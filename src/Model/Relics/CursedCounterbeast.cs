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

// 187 咒仪溯兽：攻速+5；每次作战结束后消耗一个灵感，使攻速永久+5（最多叠10次）
public sealed class CursedCounterbeast : RelicModel
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
        await SarkazRelicFx.ApplySelf<AttackSpeed>(this, 5 + 5 * Stacks);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Stacks >= 10) return;
        ThoughtModel inspirer = Owner.GetInspirers().FirstOrDefault();
        if (inspirer == null) return;
        await ThoughtCmd.Remove(inspirer);
        Stacks++;
        InvokeDisplayAmountChanged();
        Flash();
    }
}
