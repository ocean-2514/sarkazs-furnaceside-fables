using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 038 集会：使用后下一次战斗结束额外获得 10 点护盾。
public sealed class Assembly : ThoughtModel
{
    protected override int InitialRarity => 3;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Status == ThoughtStatus.Used) Owner.GainShield(10);
        await base.AfterCombatEnd(room);
    }
}