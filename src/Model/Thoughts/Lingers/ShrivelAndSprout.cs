using System.Threading.Tasks;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

// 008 枯木新枝：携带此遗愿进入下一层时再次获得该遗愿。
public sealed class ShrivelAndSprout : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;

    public override async Task AfterActEntered()
    {
        await ThoughtCmd.Obtain<ShrivelAndSprout>(Owner);
    }
}