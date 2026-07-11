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

        Traverse traverse = Traverse.Create(__instance);
        if (isPreFinished)
        {
            // 读档进入"已完成"的事件房（如从诡谲断章返回后 SL）：仿照祖先事件的
            // StartPreFinished——直接摆完成态（无选项 + Proceed），不重跑事件。
            traverse.Method("SetEventFinished", __instance.InitialDescription).GetValue();
            return false;
        }
        if (__instance.Owner.RunState.Act is BizarreFragment) return true;
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