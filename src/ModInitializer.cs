using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace Skz.Ocean;

[ModInitializer(nameof(Initializer))]
class ModInitializer
{
    public static void Initializer()
    {
        try 
        {
            var Harmony = new Harmony("ocean.sarkaz");
            Harmony.PatchAll();
        } 
        catch (Exception e)
        {
            Log.Error($"An error occurred while initializing TestMod: {e}");
            return;
        }

        Log.Info("sarkaz mod initialize");
    }
}