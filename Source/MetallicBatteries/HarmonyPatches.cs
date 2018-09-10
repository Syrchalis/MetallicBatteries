using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.AI;
using System.Reflection;

namespace MetallicBatteries
{
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = HarmonyInstance.Create("Syrchalis.Rimworld.MetallicBatteries");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
