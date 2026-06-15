using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model;
using Skz.Ocean.Model.Thoughts;

namespace Skz.Ocean.Patches;

// 测试用：新开一局时给每个玩家发放一个示例思绪。
// 之前是在 Player 构造函数 postfix 里发，太早了——那时 player.RunState 还是 null，
// 且会对每一个 Player（含预览/多人其他玩家）都发放。
// 这里改到 RunState.CreateForNewRun 之后，此时 player.RunState 已就绪。
// 注意：只覆盖“新开局”；读档(FromSerializable)不会触发，思绪存档持久化尚未实现。
// 正式发放应改为通过事件 / Neow / 奖励等入口（并用 async 的 ThoughtCmd.Obtain）。
[HarmonyPatch(typeof(RunState), nameof(RunState.CreateForNewRun))]
public static class GrantStartingThoughtsPatch
{
    public static void Postfix(RunState __result)
    {
        foreach (Player player in __result.Players)
        {
            ThoughtModel thought = SarkazHelper.ModelDbGet<MockThought>().ToMutable();
            player.AddThoughtInternal(thought);
            thought.FloorAddedToDeck = __result.TotalFloor;
            thought.Status = Entities.Thoughts.ThoughtStatus.Used;
        }
        Log.Info("[Sarkaz] Granted starting thoughts to players");
    }
}
