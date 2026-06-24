using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Factories;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 028 亡佚：使用后下次战斗结束时额外获得 1 缕灵感(Inspirer)。
public sealed class Lost : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 4;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        if (Status == ThoughtStatus.Used)
        {
            ThoughtModel inspirer = ThoughtFactory.CreateRandomInspirer(Owner, Owner.PlayerRng.Rewards);
            if (inspirer != null) await ThoughtCmd.Obtain(inspirer, Owner);
        }
        await base.AfterCombatEnd(room);
    }
}