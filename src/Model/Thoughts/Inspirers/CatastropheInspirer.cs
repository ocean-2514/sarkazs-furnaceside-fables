using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 039 天灾：使用后下一次战斗掉落的金币 +50%。
public sealed class CatastropheInspirer : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 4;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;
    public override bool CanBeUpgraded => true;
    public override Type UpgradedType => typeof(Furnace);

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Status == ThoughtStatus.Used) Owner.GetSarkazManager()?.AddGoldMultiplier(0.5f);
        await base.AfterCombatEnd(room);
    }
}