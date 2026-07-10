using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Model;
using Skz.Ocean.Model.Acts;
using Skz.Ocean.Saves;

namespace Skz.Ocean.Patches;

// 诡谲断章(支线层)里推进"下一层"时（打完子层 Boss 走出地图），
// 不进原版的 EnterNextAct（那会触发终局判定/建筑师事件），改走 PopAndReturn 回原层。
//
// 正常流程不受影响：子层 act 只在进入事件时动态追加到 RunState.Acts、
// 返回时移除，平时 Acts 里根本没有它，原版终局判定照旧。
[HarmonyPatch(typeof(RunManager), nameof(RunManager.EnterNextAct))]
public static class BizarreFragmentEnterNextActPatch
{
    public static bool Prefix(RunManager __instance, ref Task __result)
    {
        RunState state = Traverse.Create(__instance).Property("State").GetValue<RunState>();
        if (state == null || state.Act is not BizarreFragment)
        {
            return true; // 不在子层：走原版逻辑
        }
        __result = MapDetour.PopAndReturn(state);
        return false;
    }
}

// 子层美术资源沿用"来源 act"：ActModel.FilePathIdentifier 是非虚属性
// (= Id.Entry 小写，背景场景/地图贴图/篝火场景全部经它拼路径)，没法 override，
// 只能 patch 它的 getter。来源 act 从 detour 书签的 ReturnActIndex 解析（带缓存），
// 解析不到时兜底用 overgrowth——这样子层不需要自制任何场景资源。
[HarmonyPatch(typeof(ActModel), "FilePathIdentifier", MethodType.Getter)]
public static class BizarreFragmentFilePathIdentifierPatch
{
    public static bool Prefix(ActModel __instance, ref string __result)
    {
        if (__instance is not BizarreFragment)
        {
            return true;
        }
        __result = MapDetour.GetSourceActIdentifier() ?? "overgrowth";
        return false;
    }
}

// TreasureRoom 构造函数校验 actIndex 必须 0~2（校验后参数并未被使用），
// 子层是动态追加的第 4 层(索引 3)，进宝箱房会越界崩溃。
// 仅在子层里把索引替换成来源层的索引（找不到则退回 0）。
[HarmonyPatch(typeof(TreasureRoom), MethodType.Constructor, new Type[] { typeof(int) })]
public static class BizarreFragmentTreasureRoomPatch
{
    public static void Prefix(ref int actIndex)
    {
        if (actIndex >= 0 && actIndex <= 2)
        {
            return;
        }
        RunState state = Traverse.Create(RunManager.Instance).Property("State").GetValue<RunState>();
        if (state?.Act is not BizarreFragment)
        {
            return; // 非子层的越界是真 bug，让原版异常暴露出来
        }
        
        int sourceIndex = MapDetour.GetSourceAct().Index;

        actIndex = (sourceIndex >= 0 && sourceIndex <= 2) ? sourceIndex : 0;
    }
}

// 子层地图没有 Boss，但引擎要求 ActMap.BossMapPoint 非空且 NMapScreen 总会为它
// 建一个 Boss 视觉节点——在子层里把这个占位节点隐藏掉（不连通，本来也走不到）。
[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.SetMap))]
public static class BizarreFragmentHideBossPatch
{
    public static void Postfix(NMapScreen __instance)
    {
        RunState state = Traverse.Create(RunManager.Instance).Property("State").GetValue<RunState>();
        bool inFragment = state?.Act is BizarreFragment;
        NBossMapPoint bossNode = Traverse.Create(__instance).Field("_bossPointNode").GetValue<NBossMapPoint>();
        if (bossNode != null)
        {
            bossNode.Visible = !inFragment;
        }
    }
}

// NRestSiteCharacter._Ready 按 CurrentActIndex(只认 0/1/2)选 Spine 动画，
// 子层索引 3 会抛 "Unexpected act"。_Ready 执行期间把索引临时换成来源层索引——
// 既不崩，人物围火动画也自动跟来源层配套。
//
// 关键：必须绕过 CurrentActIndex 的 setter 直接写后备字段 _currentActIndex！
// setter 在值变化时会清空 _visitedMapCoords、重置 ActFloor/NextRoomId（换层语义），
// 走 setter 会把子层已走的路径抹掉，导致进完火堆后地图上永远只有起点可走。
[HarmonyPatch(typeof(NRestSiteCharacter), "_Ready")]
public static class BizarreFragmentRestSiteCharacterPatch
{
    public static void Prefix(out int __state)
    {
        __state = -1;
        RunState state = Traverse.Create(RunManager.Instance).Property("State").GetValue<RunState>();
        if (state?.Act is not BizarreFragment)
        {
            return;
        }
        ActModel source = MapDetour.GetSourceAct();
        int sourceIndex = 0;
        for (int i = 0; i < state.Acts.Count; i++)
        {
            sourceIndex = MapDetour.GetSourceAct().Index;
        }
        __state = state.CurrentActIndex;
        Traverse.Create(state).Field("_currentActIndex")
            .SetValue(sourceIndex >= 0 && sourceIndex <= 2 ? sourceIndex : 0);
    }

    // Finalizer：无论 _Ready 正常结束还是抛异常都恢复原值
    public static void Finalizer(int __state)
    {
        if (__state < 0)
        {
            return;
        }
        RunState state = Traverse.Create(RunManager.Instance).Property("State").GetValue<RunState>();
        if (state != null)
        {
            Traverse.Create(state).Field("_currentActIndex").SetValue(__state);
        }
    }
}

// 诡谲断章入口节点的任务标记换成自定义图标（%QuestIcon 的贴图写死在场景里、
// 所有任务共用；只有 Quest 来源是我们的 SarkazManagerModel 时才替换）。
// 图标文件：images/ui/bizarre_fragment_mark.png（不存在则保持原版图标）。
[HarmonyPatch(typeof(NNormalMapPoint), "_Ready")]
public static class BizarreFragmentQuestIconPatch
{
    public static void Postfix(NNormalMapPoint __instance)
    {
        MapPoint point = __instance.Point;
        if (point == null || !point.Quests.Any(q => q is SarkazManagerModel))
        {
            return;
        }
        string path = ImageHelper.GetImagePath("ui/bizarre_fragment_mark.png");
        if (!ResourceLoader.Exists(path))
        {
            return;
        }
        TextureRect questIcon = Traverse.Create(__instance).Field("_questIcon").GetValue<TextureRect>();
        if (questIcon != null)
        {
            questIcon.Texture = ResourceLoader.Load<Texture2D>(path);
        }
    }
}
