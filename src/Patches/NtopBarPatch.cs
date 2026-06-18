using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Nodes.TopBar;

namespace Skz.Ocean.Patches;


[HarmonyPatch(typeof(NTopBar), nameof(NTopBar.Initialize))]
public static class NTopBarPatch
{
    private const string _thoughtButtonPath = "res://scenes/ui/top_bar/top_bar_thought_button.tscn";
    private const string _planPath = "res://scenes/ui/top_bar/top_bar_plan.tscn";
    private const string _thoughtUsedPath = "res://scenes/ui/top_bar/top_bar_thought_used.tscn";
    private const string _thoughtButtonName = "TopBarThoughtButton";
    private const string _planName = "TopBarPlan";
    private const string _thoughtUsedName = "TopBarThoughtUsed";

    private const string _containerPath = "LeftAlignedStuff";

    [HarmonyPostfix]
    public static void Postfix(NTopBar __instance, IRunState runState)
    {
        Control? container = __instance.GetNodeOrNull<Control>(_containerPath);
        if (container == null)
        {
            Log.Warn("[Sarkaz] top bar container '" + _containerPath + "' not found");
            return;
        }

        if (container.GetNodeOrNull(_thoughtButtonName) != null) return;
        if (container.GetNodeOrNull(_planName) != null) return;    
        if (container.GetNodeOrNull(_thoughtUsedName) != null) return;    

        AddToTopBar(_thoughtButtonPath, _thoughtButtonName, container, runState);
        AddToTopBar(_planPath, _planName, container, runState);    
        AddToTopBar(_thoughtUsedPath, _thoughtUsedName, container, runState);
    }

    private static void AddToTopBar(string scenePath, string sceneName, Control container, IRunState runState)
    {
        PackedScene? scene = ResourceLoader.Load<PackedScene>(scenePath);
        if (scene == null)
        {
            Log.Warn("[Sarkaz] failed to load " + scenePath);
            return;
        }

        Node instance = scene.Instantiate();
        if (instance is not ITopBarScene node)
        {
            Log.Warn($"根节点没有挂 ITopBarScene 脚本");
            instance.QueueFree();
            return;
        }

        instance.Name = sceneName;
        container.AddChild(instance);
        node.Initialize(LocalContext.GetMe(runState));
        Log.Info("Added top bar scene to top bar");
    }
}
