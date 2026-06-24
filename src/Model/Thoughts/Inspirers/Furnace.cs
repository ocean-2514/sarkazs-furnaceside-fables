using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 040 熔炉：使用后下一次战斗掉落的金币 +100%。
public sealed class Furnace : ThoughtModel
{
    protected override int InitialRarity => 3;
    protected override int InitialToil => 4;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Rare;

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Status == ThoughtStatus.Used) Owner.GetSarkazManager()?.AddGoldMultiplier(1.0f);
        await base.AfterCombatEnd(room);
    }
}