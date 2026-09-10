namespace Skz.Ocean.Patches;

using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using Skz.Ocean.Entities.Ascension;
using Skz.Ocean.Nodes.Screens;

[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.InitializeSingleplayer))]
public static class SarkazAscensionPanelPatch
{
    private const string ScenePath = "res://scenes/screens/sarkaz_ascension_panel.tscn";

    public static void Postfix(NCharacterSelectScreen __instance)
    {
        if (__instance.GetNodeOrNull<NSarkazAscensionPanel>("SarkazAscensionPanel") != null)
            return;

        PackedScene scene = GD.Load<PackedScene>(ScenePath);
        if (scene == null)
        {
            Log.Error($"[Sarkaz] Failed to load {ScenePath}");
            return;
        }

        NSarkazAscensionPanel panel = scene.Instantiate<NSarkazAscensionPanel>(
            PackedScene.GenEditState.Disabled);
        panel.Name = "SarkazAscensionPanel";
        __instance.AddChild(panel);

        int preferred = SarkazAscensionPreferencesIO.LoadPreferredLevel();
        SarkazAscensionState.Select(preferred);
        panel.Initialize(preferred);
        panel.Connect(NSarkazAscensionPanel.SignalName.AscensionLevelChanged,
            Callable.From(() =>
            {
                SarkazAscensionState.Select(panel.Ascension);
                SarkazAscensionPreferencesIO.SavePreferredLevel(panel.Ascension);
            }));
    }
}
