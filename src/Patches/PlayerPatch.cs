using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model;
using Skz.Ocean.Model.Thoughts;

namespace Skz.Ocean.Patches;

[HarmonyPatch]
public static class PlayerConstructorPatch
{
    [HarmonyTargetMethod]
    public static ConstructorInfo TargetMethod()
    {
        // 关键：需要指定 BindingFlags 来获取私有构造函数
        var constructor = typeof(Player).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,  // NonPublic 获取 private
            null,
            new Type[]
            {
                typeof(CharacterModel),           // character
                typeof(ulong),                    // netId
                typeof(int),                      // currentHp
                typeof(int),                      // maxHp
                typeof(int),                      // maxEnergy
                typeof(int),                      // gold
                typeof(int),                      // potionSlotCount
                typeof(int),                      // orbSlotCount
                typeof(RelicGrabBag),             // sharedRelicGrabBag
                typeof(UnlockState),              // unlockState
                typeof(List<ModelId>),            // discoveredCards (可空但类型相同)
                typeof(List<ModelId>),            // discoveredEnemies
                typeof(List<string>),             // discoveredEpochs
                typeof(List<ModelId>),            // discoveredPotions
                typeof(List<ModelId>)             // discoveredRelics
            },
            null
        );
        return constructor;
    }
    
    public static async Task Postfix(Player __instance)
    {
        Log.Info("Player constructor postfix called!");
        __instance.AddThoughtInternal(SarkazHelper.ModelDbGet<MockThought>().ToMutable());
        ThoughtModel thought = __instance.GetInpirers()[0];
        Log.Info(thought.Title.ToString() + " " + thought.Description.ToString());
        await __instance.UseThought(thought);
    }
}