using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 037 话语：使用后下一次战斗结束额外获得 5 点护盾。
public sealed class Words : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;

    public override bool CanBeUpgraded => true;
    public override Type UpgradedType => typeof(Assembly);

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Status == ThoughtStatus.Used) Owner.GainShield(5);
        await base.AfterCombatEnd(room);
    }
}