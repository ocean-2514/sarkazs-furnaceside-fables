using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Nodes;

namespace Skz.Ocean.Patches;


[HarmonyPatch(typeof(NTopBar), nameof(NTopBar.Initialize))]
public static class NTopBarPatch
{
    private const string _scenePath = "res://scenes/ui/top_bar/top_bar_thought_button.tscn";
    private const string _nodeName = "TopBarThoughtButton";

    // 放在哪个容器里：左边一排(HP/金币/药水) = "LeftAlignedStuff"；右边(地图/牌堆/暂停) = "RightAlignedStuff"
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

        if (container.GetNodeOrNull(_nodeName) != null)
        {
            return;
        }

        PackedScene? scene = ResourceLoader.Load<PackedScene>(_scenePath);
        if (scene == null)
        {
            Log.Warn("[Sarkaz] failed to load " + _scenePath);
            return;
        }

        Node instance = scene.Instantiate();
        if (instance is not NTopBarThoughtButton node)
        {
            Log.Warn("根节点没有挂 NTopBarThoughtButton 脚本");
            instance.QueueFree();
            return;
        }

        node.Name = _nodeName;
        container.AddChild(node);
        node.Initialize(LocalContext.GetMe(runState));
        Log.Info("Added top bar thought button to top bar");
    }
}
