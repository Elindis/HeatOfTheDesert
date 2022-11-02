using HarmonyLib;
using HeatOfTheDesert;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Verse;

namespace ExpandablePlants
{
    // I'm 99% sure I don't need this. I could create a blank window and concatenate a bunch of entries instead.
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony("elindis.heatofthedesert");
            harmony.Patch(AccessTools.Method(typeof(PlantProperties), "SpecialDisplayStats"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(RemoveConstantGrowthTemperatureDisplay)));
            harmony.Patch(AccessTools.Method(typeof(ThingDef), "SpecialDisplayStats"),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(AddTrueGrowthTemperatureDisplay)));
        }

        public static string OldMinGrowthTemperatureEntryLabel => "MinGrowthTemperature".Translate().CapitalizeFirst();
        public static string OldMaxGrowthTemperatureEntryLabel => "MaxGrowthTemperature".Translate().CapitalizeFirst();

        // Keep all StatDrawEntries except those with the growth temperature labels.
        private static void RemoveConstantGrowthTemperatureDisplay(ref IEnumerable<StatDrawEntry> __result)
        {
            __result = __result.Where((StatDrawEntry entry) =>
            {
                if (entry.LabelCap.Equals(OldMinGrowthTemperatureEntryLabel) || entry.LabelCap.Equals(OldMaxGrowthTemperatureEntryLabel))
                {
                    return false;
                }
                return true;
            });
        }

        // Add new growth temperature labels based on the true growth temperature of the plant.
        private static void AddTrueGrowthTemperatureDisplay(ThingDef __instance, ref IEnumerable<StatDrawEntry> __result)
        {
            // Skip non-plants.
            if (__instance.plant != null)
            {
                float minGrowthTemperature;
                float maxGrowthTemperature;

                // Determine whether this is a fragile plant or a regular plant.
                HeatOfTheDesertExtension Props = __instance.GetModExtension<HeatOfTheDesertExtension>();
                if (Props == null)
                {
                    // Regular plants use RimWorld's constant growth temperatures.
                    minGrowthTemperature = Plant.MinGrowthTemperature;
                    maxGrowthTemperature = Plant.MaxGrowthTemperature;
                }
                else
                {
                    // ExpandablePlants plants get their growth temperatures from the component properties.
                    minGrowthTemperature = Plant.MinGrowthTemperature;
                    maxGrowthTemperature = Props.deathTemperature;
                }

                __result = __result.Concat(new[] {
                    new StatDrawEntry(StatCategoryDefOf.Basics, "MinGrowthTemperature".Translate(), minGrowthTemperature.ToStringTemperature(), "Stat_Thing_Plant_MinGrowthTemperature_Desc".Translate(), 4152),
                    new StatDrawEntry(StatCategoryDefOf.Basics, "MaxGrowthTemperature".Translate(), maxGrowthTemperature.ToStringTemperature(), "Stat_Thing_Plant_MaxGrowthTemperature_Desc".Translate(), 4153)
                });
            }
        }
    }
}
