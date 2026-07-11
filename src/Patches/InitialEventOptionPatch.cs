using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Model.Acts;
using Skz.Ocean.Model.Events;

namespace Skz.Ocean.Patches;

[HarmonyPatch]
public static class InitialEventOptionPatch
{
    static MethodBase TargetMethod()
    {
        return typeof(EventModel).GetMethod("SetInitialEventState", 
            BindingFlags.NonPublic | BindingFlags.Instance);
    }

    [HarmonyPrefix] 
    public static bool Prefix(EventModel __instance, bool isPreFinished) 
    {
        Log.Info("[sarkaz] Patching...");

        if (__instance is AncientEventModel) return true;
        if (__instance.Owner.RunState.Act is BizarreFragment) return true;

        if (isPreFinished)
		{
			throw new InvalidOperationException($"Tried to load into pre-finished event {__instance}! Only ancient events can be pre-finished.");
		}
        Traverse traverse = Traverse.Create(__instance);
        List<EventOption> options =
        [
            .. traverse.Method("GenerateInitialOptionsWrapper").
                GetValue<IReadOnlyList<EventOption>>(),
            SarkazCommonEvent.CreateCommonOption(__instance),
        ];

        traverse.Method("SetEventState", __instance.InitialDescription, options).GetValue();

        return false;
    }
}