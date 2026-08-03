using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Map;
using Skz.Ocean.Model.Acts;
using Skz.Ocean.Saves;

namespace Skz.Ocean.Patches;

[HarmonyPatch]
public static class NRewardsScreenPatch
{
    static MethodBase TargetMethod()
    {
        return typeof(NRewardsScreen).GetMethod("OnProceedButtonPressed", 
            BindingFlags.NonPublic | BindingFlags.Instance);
    }

    [HarmonyPrefix]
    public static bool Prefix(NRewardsScreen __instance, NButton _)
    {
        Traverse trav = Traverse.Create(__instance);
        bool isTerminal = trav.Field("_isTerminal").GetValue<bool>();
        IRunState runState = trav.Field("_runState").GetValue<IRunState>();
        if (isTerminal && runState.Act is BizarreFragment 
            && BizarreFragmentActMap.IsExitMapPoint(runState.CurrentMapPoint))
        {
            TaskHelper.RunSafely(MapDetour.PopAndReturn(runState as RunState));
            return false;
        }
        return true;
    }
}