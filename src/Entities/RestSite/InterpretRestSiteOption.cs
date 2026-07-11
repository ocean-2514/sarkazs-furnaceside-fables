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
}
