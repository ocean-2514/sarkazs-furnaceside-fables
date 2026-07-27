namespace  Skz.Ocean.Patches;

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using Skz.Ocean.Model.Encounters;
using Skz.Ocean.Model.Encounters.Mocks;
using Skz.Ocean.Model.Monsters;

[HarmonyPatch(typeof(Overgrowth), nameof(Overgrowth.GenerateAllEncounters))]
public static class OvergrowthGenerateAllEncountersPatch
{
    static void Postfix(ref IEnumerable<EncounterModel> __result)
    {
        __result = __result
            .Concat(new EncounterModel[] {
                ModelDb.Encounter<BadNeighborsNormal>(),
                ModelDb.Encounter<HastyEscape>(),
                ModelDb.Encounter<VerminWeak>(),
                ModelDb.Encounter<IceAgeNormal>(),
                ModelDb.Encounter<TemporaryPatchupElite>(),
                ModelDb.Encounter<ExhaustVentsNormal>(),
                ModelDb.Encounter<FurnaceVolunteersNormal>(),
                ModelDb.Encounter<CleartheCrowdElite>(),
                ModelDb.Encounter<DissuasionSeminarNormal>(),
                ModelDb.Encounter<DenseJungleElite>(),
            })
            .Distinct();
        __result = new EncounterModel[] {
            ModelDb.Encounter<BadNeighborsNormal>(),
            ModelDb.Encounter<HastyEscape>(),
            ModelDb.Encounter<VerminWeak>(),
            ModelDb.Encounter<IceAgeNormal>(),
            ModelDb.Encounter<TemporaryPatchupElite>(),
            ModelDb.Encounter<ExhaustVentsNormal>(),
            ModelDb.Encounter<FurnaceVolunteersNormal>(),
            ModelDb.Encounter<CleartheCrowdElite>(),
            ModelDb.Encounter<DissuasionSeminarNormal>(),
            ModelDb.Encounter<DenseJungleElite>(),
            ModelDb.Encounter<CeremonialBeastBoss>(),
        };
    }
}


[HarmonyPatch(typeof(Underdocks), nameof(Overgrowth.GenerateAllEncounters))]
public static class UnderdocksGenerateAllEncountersPatch
{
    static void Postfix(ref IEnumerable<EncounterModel> __result)
    {
        __result = __result
            .Concat(new EncounterModel[] {
                ModelDb.Encounter<BadNeighborsNormal>(),
                ModelDb.Encounter<HastyEscape>(),
                ModelDb.Encounter<VerminWeak>(),
                ModelDb.Encounter<IceAgeNormal>(),
                ModelDb.Encounter<TemporaryPatchupElite>(),
                ModelDb.Encounter<ExhaustVentsNormal>(),
                ModelDb.Encounter<FurnaceVolunteersNormal>(),
                ModelDb.Encounter<CleartheCrowdElite>(),
                ModelDb.Encounter<DissuasionSeminarNormal>(),
                ModelDb.Encounter<DenseJungleElite>(),
            })
            .Distinct();
        __result = new EncounterModel[] {
            ModelDb.Encounter<BadNeighborsNormal>(),
            ModelDb.Encounter<HastyEscape>(),
            ModelDb.Encounter<VerminWeak>(),
            ModelDb.Encounter<IceAgeNormal>(),
            ModelDb.Encounter<TemporaryPatchupElite>(),
            ModelDb.Encounter<ExhaustVentsNormal>(),
            ModelDb.Encounter<FurnaceVolunteersNormal>(),
            ModelDb.Encounter<CleartheCrowdElite>(),
            ModelDb.Encounter<DissuasionSeminarNormal>(),
            ModelDb.Encounter<DenseJungleElite>(),
            ModelDb.Encounter<CeremonialBeastBoss>(),
        };
    }
}