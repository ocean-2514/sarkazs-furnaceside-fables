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
using MegaCrit.Sts2.Core.Map;
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
using Skz.Ocean.Model.Powers;
using Skz.Ocean.Model.Relics.Base;
using Skz.Ocean.Model.Thoughts.Inspirers;

namespace Skz.Ocean.Model.Relics;

// 190 魔王的祭器：目标生命值达到上限时，进入战斗时基础生命值+80%（本场临时），
// 集齐3件及以上魔王收藏品（床榻/旗帜/祭器）后变为+200%
public sealed class KingOfSarkazsVessel : RelicModel
{
    private int _granted;

    public override RelicRarity Rarity => RelicRarity.Rare;

    private int SarkazKingSetCount => Owner.Relics.Count(r =>
        r is SarkazKingsRegalRest || r is SarkazKingsTornBanner || r is KingOfSarkazsVessel);

    public override async Task BeforeCombatStart()
    {
        if (Owner.Creature.CurrentHp < Owner.Creature.MaxHp) return;
        int pct = SarkazKingSetCount >= 3 ? 200 : 80;
        Flash();
        _granted = Owner.Character.StartingHp * pct / 100;
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
