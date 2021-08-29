using UnityEngine;
using RimWorld;
using Verse;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using JetBrains.Annotations;
using HarmonyLib;

// ReSharper disable InconsistentNaming

namespace MetallicBatteries
{
    [HarmonyPatch(typeof(Building_Battery), nameof(Building_Battery.Draw))]
    [UsedImplicitly]
    public static class Building_BatteryPatch
    {
        // METHOD OLD IL:
        /*
        IL_005b: ldloca.s     r
        IL_005d: ldsfld       class [UnityEngine.CoreModule]UnityEngine.Material RimWorld.Building_Battery::BatteryBarFilledMat
        IL_0062: stfld        class [UnityEngine.CoreModule]UnityEngine.Material Verse.GenDraw/FillableBarRequest::filledMat
         */

        // METHOD TARGET IL:
        /* 
        IL_005B: ldloca.s  r
        IL_005D: ldloc.1
        IL_005E: ldfld     float32 Verse.GenDraw/FillableBarRequest::fillPercent
        IL_0063: call      class [UnityEngine.CoreModule]UnityEngine.Material [MetallicBatteries]MetallicBatteries.Building_BatteryPatch::GetMatColor(float32)
        IL_0068: stfld     class [UnityEngine.CoreModule]UnityEngine.Material Verse.GenDraw/FillableBarRequest::filledMat
         */

        [HarmonyTranspiler]
        [UsedImplicitly]
        public static IEnumerable<CodeInstruction> Transpiler([NotNull] IEnumerable<CodeInstruction> instructions)
        {
            var batteryBarFilledMat = AccessTools.Field(typeof(Building_Battery), "BatteryBarFilledMat");

            foreach (var i in instructions.ToList())
            {
                if (i.opcode == OpCodes.Ldsfld && (FieldInfo) i.operand == batteryBarFilledMat)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldfld, typeof(GenDraw.FillableBarRequest).GetField(nameof(GenDraw.FillableBarRequest.fillPercent)));
                    yield return new CodeInstruction(OpCodes.Call, typeof(Building_BatteryPatch).GetMethod(nameof(GetMatColor)));

                    continue;
                }

                yield return i;
            }
        }

        public static Material GetMatColor(float fillPercent)
        {
            return SolidColorMaterials.SimpleSolidColorMaterial(
                new Color(GenMath.LerpDouble(0.5f, 1f, 1f, 0f, fillPercent),
                    GenMath.LerpDouble(0f, 0.5f, 0f, 1f, fillPercent), 0.2f));
        }
    }

    [HarmonyPatch(typeof(CompPowerBattery))]
    [UsedImplicitly]
    public static class CompPowerBatteryPatches
    {
        [HarmonyPatch(nameof(CompPowerBattery.AddEnergy))]
        [HarmonyPrefix]
        [UsedImplicitly]
        public static void AddEnergy_Prefix(ref float amount, [CanBeNull] CompPowerBattery __instance)
        {
            if (__instance?.parent?.def != null && __instance.parent.def == MetallicBatteriesDefOf.Battery_Uranium)
                amount *= Mathf.InverseLerp(80f, 20f, __instance.parent.AmbientTemperature);
        }

        [HarmonyPatch(nameof(CompPowerBattery.CompInspectStringExtra))]
        [HarmonyPostfix]
        [UsedImplicitly]
        public static void CompInspectStringExtra_Postfix(ref string __result, [CanBeNull] CompPowerBattery __instance)
        {
            if (__instance?.parent?.def != null && __instance.parent.def == MetallicBatteriesDefOf.Battery_Uranium)
                __result += "\n" + "PowerBatteryHeatLoss".Translate() + ": " +
                            (__instance.Props.efficiency * 100f *
                             Mathf.InverseLerp(80f, 20f, __instance.parent.AmbientTemperature)).ToString("F0") + "%";
        }
    }
}
