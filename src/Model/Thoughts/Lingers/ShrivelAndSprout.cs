using System.Threading.Tasks;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Model.Acts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

// 008 枯木新枝：携带此遗愿进入下一层时再次获得该遗愿。
public sealed class ShrivelAndSprout : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Rare;

    public override async Task AfterActEntered()
    {
        if (Owner.GetSarkazManager().PreviousActWasFragment) return;
        if (Owner.RunState is not MegaCrit.Sts2.Core.Runs.RunState state) return;
        if (state.Act is BizarreFragment) return;
        await ThoughtCmd.Obtain<ShrivelAndSprout>(Owner);
    }
}