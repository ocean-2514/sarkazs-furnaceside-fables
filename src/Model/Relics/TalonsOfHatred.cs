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

// 155 死仇时代的恨意：所有友方单位和敌人攻击力+20点，刷新节点时更容易出现精英作战。获得[gold]Friston.P[/gold]。
public sealed class TalonsOfHatred : EnemyDebuffRelic, ISarkazModel
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        await RelicCmd.Obtain<FristonP>(Owner);
    }


    public override async Task BeforeCombatStart()
    {
        await SarkazRelicFx.ApplySelf<AttackPower>(this, 20);
    }

    protected override async Task ApplyToEnemy(Creature enemy)
    {
        await SarkazRelicFx.ApplyPower<AttackPower>(this, enemy, 20);
    }

    public MapPointType ModifyRefreshedPointType(IRunState runState, MapPointType pointType, MapPointType initialType)
    {
        if (pointType == MapPointType.Elite || initialType == MapPointType.Elite) return pointType;
        Rng rng = (runState as RunState)?.Rng.Niche;
        if (rng != null && rng.NextFloat() < 0.8f) return MapPointType.Elite;
        return pointType;
    }
}
