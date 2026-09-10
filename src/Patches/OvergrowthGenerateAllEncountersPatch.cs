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
    public static IEnumerable<EncounterModel> SarkazEncounters1 = [
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
    ];

    public static IEnumerable<EncounterModel> SarkazEncounters2 = [
        ModelDb.Encounter<BloodlineDebatesNormal>(),
        ModelDb.Encounter<BlottedSkyNormal>(),
        ModelDb.Encounter<MorningLaborNormal>(),
        ModelDb.Encounter<CarelessMagicNormal>(),
        ModelDb.Encounter<FirepowerSquadNormal>(),
        ModelDb.Encounter<EpochalGapsElite>(),
        ModelDb.Encounter<SordidSurveillanceElite>(),   //normal
        ModelDb.Encounter<RulesOfModernWarfareNormal>(),
        ModelDb.Encounter<TimewornTensionsElite>(),
        ModelDb.Encounter<ElusiveMercElite>(),
        ModelDb.Encounter<ChaosElite>(),
    ];

    public static IEnumerable<EncounterModel> SarkazEncounters3 = [
        ModelDb.Encounter<LandReclamationNormal>(),
        ModelDb.Encounter<WitchCurseAllianceNormal>(),
        ModelDb.Encounter<PassageBlockadeNormal>(),
        ModelDb.Encounter<GuiltlessParadiseNormal>(),
        ModelDb.Encounter<AerialAssaultNormal>(),
        ModelDb.Encounter<RegularArmyElite>(),
        ModelDb.Encounter<RhineGuardiansElite>(),
    ];

    static void Postfix(ref IEnumerable<EncounterModel> __result)
    {
        __result = __result
            .Concat(SarkazEncounters1)
            .Distinct(); 
        // __result = SarkazEncounters1.Concat([ ModelDb.Encounter<CeremonialBeastBoss>() ]);
        // __result = [
        //     ModelDb.Encounter<DenseJungleElite>(),
        //     ModelDb.Encounter<CeremonialBeastBoss>(),
        // ];
    }
}


[HarmonyPatch(typeof(Underdocks), nameof(Overgrowth.GenerateAllEncounters))]
public static class UnderdocksGenerateAllEncountersPatch
{
    static void Postfix(ref IEnumerable<EncounterModel> __result)
    {
        __result = __result
            .Concat(OvergrowthGenerateAllEncountersPatch.SarkazEncounters1)
            .Distinct();
        // __result = OvergrowthGenerateAllEncountersPatch.SarkazEncounters1.Concat([ ModelDb.Encounter<CeremonialBeastBoss>() ]);
        // __result = [
        //     ModelDb.Encounter<DenseJungleElite>(),
        //     ModelDb.Encounter<CeremonialBeastBoss>(),
        // ];
    }
}

[HarmonyPatch(typeof(Hive), nameof(Hive.GenerateAllEncounters))]
public static class HiveGenerateAllEncountersPatch
{
    static void Postfix(ref IEnumerable<EncounterModel> __result)
    {
        __result = __result
            .Concat(OvergrowthGenerateAllEncountersPatch.SarkazEncounters2)
            .Distinct();
        // __result = OvergrowthGenerateAllEncountersPatch.SarkazEncounters2.Concat([ ModelDb.Encounter<CeremonialBeastBoss>() ]);
        // __result = [
        //     ModelDb.Encounter<MockSpinesOfEpoch>(),
        //     ModelDb.Encounter<CeremonialBeastBoss>(),
        // ];
    }
}

[HarmonyPatch(typeof(Glory), nameof(Glory.GenerateAllEncounters))]
public static class GloryGenerateAllEncountersPatch
{
    static void Postfix(ref IEnumerable<EncounterModel> __result)
    {
        __result = __result
            .Concat(OvergrowthGenerateAllEncountersPatch.SarkazEncounters3)
            .Distinct();
        // __result = OvergrowthGenerateAllEncountersPatch.SarkazEncounters3.Concat([ ModelDb.Encounter<CeremonialBeastBoss>() ]);
        // __result = [
        //     ModelDb.Encounter<MockSpinesOfEpoch>(),
        //     ModelDb.Encounter<CeremonialBeastBoss>(),
        // ];
    }
}
