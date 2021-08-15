using Verse;
using System.Reflection;
using JetBrains.Annotations;
using HarmonyLib;

namespace MetallicBatteries
{
    [StaticConstructorOnStartup]
    [UsedImplicitly]
    public class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("Syrchalis.Rimworld.MetallicBatteries");

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
