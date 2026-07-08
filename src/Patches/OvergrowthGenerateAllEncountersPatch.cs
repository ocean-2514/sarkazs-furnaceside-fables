// namespace  Skz.Ocean.Patches;

// using System.Collections.Generic;
// using System.Linq;
// using HarmonyLib;
// using MegaCrit.Sts2.Core.Models;
// using MegaCrit.Sts2.Core.Models.Acts;
// using MegaCrit.Sts2.Core.Models.Encounters;
// using Skz.Ocean.Model.Encounters.Mocks;
// using Skz.Ocean.Model.Monsters;

// [HarmonyPatch(typeof(Overgrowth), nameof(Overgrowth.GenerateAllEncounters))]
// public static class OvergrowthGenerateAllEncountersPatch
// {
//     static void Postfix(ref IEnumerable<EncounterModel> __result)
//     {
//         __result = __result
//             .Concat(new[] { ModelDb.Encounter<MockSpinesOfEpoch>() })
//             .Distinct();
//         __result = new EncounterModel[] { ModelDb.Encounter<ThievingHopperWeak>(), ModelDb.Encounter<GremlinMercNormal>() };
//     }
// }