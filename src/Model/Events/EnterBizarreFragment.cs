using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Model.Thoughts.Plans;
using Skz.Ocean.Saves;

namespace Skz.Ocean.Model.Events;

// 进入"诡谲断章"支线层的事件。不进任何 act 的事件池——由 SarkazManagerModel
// 在每层地图后半段钉住的固定节点上强制触发(ModifyUnknownMapPointRoomTypes + ModifyNextEvent)。
public sealed class EnterBizarreFragment : EventModel
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return [
            GenerateOptions(),
            new EventOption(this, LeaveInternal, InitialOptionKey("LEAVE")),
        ];
    }

    private EventOption GenerateOptions()
    {
        if (Owner.GetPlanCount() >= 2) 
        {
            return new EventOption(this, EnterInternal, InitialOptionKey("ENTER_FRAGMENT"));
        }
        else return new EventOption(this, null, InitialOptionKey("LOCKED"));
    }

    private Task EnterInternal()
    {
        Owner.RemovePlans(2);
        SetEventFinished(L10NLookup("ENTER_BIZARRE_FRAGMENT.pages.EXIT.description"));

        // 不能在选项 task 里 await 切层：EnterAct 会退出本事件房间，而
        // EventRoom.Exit → EventSynchronizer.AwaitPendingOptionTasks 会等本选项 task
        // 完成 → 自己等自己死锁。拆成 fire-and-forget，选项 task 先结束，
        // 切层（压书签 → 追加子层 act → EnterAct）在下一个异步节拍执行。
        TaskHelper.RunSafely(EnterFragmentDetached(Owner.RunState as RunState, Owner));
        return Task.CompletedTask;
    }

    private static async Task EnterFragmentDetached(RunState state, Player player)
    {
        await Task.Yield(); // 等本次选项调用栈完全退出再开始切层
        await MapDetour.EnterFragment(state, player);
    }

    private async Task LeaveInternal()
    {
        await ThoughtCmd.Obtain<Plan>(Owner);
        SetEventFinished(L10NLookup("ENTER_BIZARRE_FRAGMENT.pages.LEAVE.description"));
    }
}
