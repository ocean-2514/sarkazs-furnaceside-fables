using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Saves;

namespace Skz.Ocean.Model.Events;

// 离开"诡谲断章"：弹出书签，回到原层原节点继续选路。
// 挂在 BizarreFragment act 的事件池里；子层 Boss 打完推进下一层时也会经
// EnterNextAct 补丁走同一条 PopAndReturn 路径。
public sealed class ExitBizarreFragment : EventModel
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return [
            new EventOption(this, ExitInternal, InitialOptionKey("EXIT_FRAGMENT"))
        ];
    }

    private Task ExitInternal()
    {
        SetEventFinished(L10NLookup("EXIT_BIZARRE_FRAGMENT.pages.EXIT.description"));

        // 同 EnterBizarreFragment：切层会退出本事件房间并等待所有选项 task 完成，
        // 在选项 task 里 await 会自己等自己死锁。拆成 fire-and-forget。
        TaskHelper.RunSafely(PopAndReturnDetached(Owner.RunState as RunState));
        return Task.CompletedTask;
    }

    private static async Task PopAndReturnDetached(RunState state)
    {
        await Task.Yield(); // 等本次选项调用栈完全退出再开始返回
        await MapDetour.PopAndReturn(state);
    }
}
