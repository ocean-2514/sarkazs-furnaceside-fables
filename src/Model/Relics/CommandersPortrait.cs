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

// 144 统帅肖像：攻击力、基础生命值+10%；BOSS战中额外+20%
// 实现：拾取时基础生命永久+10%；每场战斗攻击力+10%（Boss +30%）；Boss战再临时+20%基础生命（战后移除）
public sealed class CommandersPortrait : RelicModel
{
    private int _granted;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        await SarkazRelicFx.GainBaseHpPercent(Owner, 10);
    }

    public override async Task BeforeCombatStart()
    {
        bool boss = Owner.RunState.CurrentRoom?.RoomType == RoomType.Boss;
        await SarkazRelicFx.ApplySelf<AttackPower>(this, boss ? 30 : 10);
        if (boss)
        {
            _granted = Owner.Character.StartingHp * 20 / 100;
            if (_granted > 0) await CreatureCmd.GainMaxHp(Owner.Creature, _granted);
        }
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
