using HarmonyLib;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;
using Verse.Sound;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace MetallicBatteries
{
    [HarmonyPatch(typeof(Building_Battery), "Draw")]
    public static class Building_BatteryPatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo BatteryBarFilledMat = AccessTools.Field(typeof(Building_Battery), "BatteryBarFilledMat");
            //FieldInfo fillPercent = AccessTools.Field(typeof(GenDraw.FillableBarRequest), "fillPercent");
            MethodInfo MatColor = AccessTools.Method(typeof(Building_BatteryPatch), nameof(GetMatColor));
            foreach (CodeInstruction i in instructions)
            {
                if (i.opcode == OpCodes.Ldsfld && (FieldInfo)i.operand == BatteryBarFilledMat)
                {
                    //yield return new CodeInstruction(instructions.ElementAt(5));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, MatColor);
                    continue;
                }
                yield return i;
            } 
        }

        public static Material GetMatColor(CompPowerBattery comp)
        {
            float fillPercent = comp.StoredEnergy / comp.Props.storedEnergyMax;
            return SolidColorMaterials.SimpleSolidColorMaterial(new Color(GenMath.LerpDouble(0.5f, 1f, 1f, 0f, fillPercent), GenMath.LerpDouble(0f, 0.5f, 0f, 1f, fillPercent), 0.2f), false);
        }
    }

    [HarmonyPatch(typeof(CompPowerBattery), nameof(CompPowerBattery.AddEnergy))]
    public static class AddEnergyPatch
    {
        [HarmonyPrefix]
        public static void AddEnergy_Prefix(ref float amount, CompPowerBattery __instance)
        {
            if (__instance?.parent?.def != null && __instance.parent.def == MetallicBatteriesDefOf.Battery_Uranium)
            {
                amount *= Mathf.InverseLerp(80f, 20f, __instance.parent.AmbientTemperature);
            }
        }
    }

    [HarmonyPatch(typeof(CompPowerBattery), nameof(CompPowerBattery.CompInspectStringExtra))]
    public static class CompInspectStringExtraPatch
    {
        [HarmonyPostfix]
        public static void CompInspectStringExtra_Postfix(ref string __result, CompPowerBattery __instance)
        {
            if (__instance?.parent?.def != null && __instance.parent.def == MetallicBatteriesDefOf.Battery_Uranium)
            {
                __result += "\n" + "PowerBatteryHeatLoss".Translate() + ": " + (__instance.Props.efficiency * 100f * Mathf.InverseLerp(80f, 20f, __instance.parent.AmbientTemperature)).ToString("F0") + "%";
            }
        }
    }
}
