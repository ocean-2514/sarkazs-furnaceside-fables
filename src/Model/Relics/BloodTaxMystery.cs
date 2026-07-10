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

// 158 血税之谜：所有友方单位和敌人的最大生命值减少35%，仅诡谲断章内生效。所有敌人生命值+10%。
public sealed class BloodTaxMystery : FragmentMysteryRelic
{
    private int _playerLoss;

    public override async Task BeforeCombatStart()
    {
        if (!InFragment) return;
        _playerLoss = (int)(Owner.Creature.MaxHp * 35 / 100);
        if (_playerLoss > 0)
        {
            await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), Owner.Creature, _playerLoss, isFromCard: false);
        }
    }

    protected override async Task OnEnemyRegistered(Creature enemy)
    {
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), enemy, enemy.MaxHp * 35 / 100, isFromCard: false);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await base.AfterCombatEnd(room);
        if (_playerLoss > 0 && Owner.Creature.CurrentHp > 0)
        {
            await CreatureCmd.GainMaxHp(Owner.Creature, _playerLoss);
        }
        _playerLoss = 0;
    }
}
