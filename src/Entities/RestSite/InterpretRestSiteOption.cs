using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Nodes.Screens;

namespace Skz.Ocean.Entities.RestSite;

public class InterpretRestSiteOption : RestSiteOption
{
    public override string OptionId => "INTERPRET";

    public InterpretRestSiteOption(Player player) :
        base(player) {}

    public override Task<bool> OnSelect()
    {
        // 界面没开出来（Capstone 不可用等）就算失败：不消耗选项，按钮会重新启用。
        bool opened = NThoughtInterpretScreen.ShowScreen(base.Owner) != null;
        return Task.FromResult(opened);
    }

    // 选中成功后，原版会把所有选项 Clear()（其他选项消失，符合预期）。
    // 这个回调恰好在 Clear() 之后、UpdateRestSiteOptions() 重建按钮之前执行，
    // 把"解读"自己加回去 → 按钮重建后只剩解读，可重复进入。
    // 选其他选项时不经过这里，走原版全清逻辑。
    public override Task DoLocalPostSelectVfx(CancellationToken ct = default)
    {
        if (RunManager.Instance.RestSiteSynchronizer.GetOptionsForPlayer(base.Owner)
                is List<RestSiteOption> options
            && !options.Contains(this))
        {
            options.Add(this);
        }
        return Task.CompletedTask;
    }
}
