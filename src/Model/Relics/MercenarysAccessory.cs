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

// 135 佣兵的饰物：完美作战（没有因敌方攻击损失生命值）结束后攻击力、基础生命值永久+5%（最多10层）
public sealed class MercenarysAccessory : RelicModel
{
    private int _stacks;

    public override RelicRarity Rarity => RelicRarity.Rare;

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
        if (Stacks > 0) await SarkazRelicFx.ApplySelf<AttackPower>(this, Stacks * 5);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Stacks >= 10) return;
        if (Owner.GetCombatManager()?.WasPerfectCombat != true) return;
        Stacks++;
        InvokeDisplayAmountChanged();
        Flash();
        await SarkazRelicFx.GainBaseHpPercent(Owner, 5);
    }
}
