using RimWorld;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace HeatOfTheDesert
{
    public class Fragile_Plant : Plant
    {
        public HeatOfTheDesertExtension Props => def.GetModExtension<HeatOfTheDesertExtension>();
        public float deathTemperature => def.GetModExtension<HeatOfTheDesertExtension>().deathTemperature;
        public float maxOptimalTemperature => def.GetModExtension<HeatOfTheDesertExtension>().maxOptimalTemperature;
        public float maxGrowthTemperature => def.GetModExtension<HeatOfTheDesertExtension>().maxGrowthTemperature;
        public bool diesInHeat => def.GetModExtension<HeatOfTheDesertExtension>().diesInHeat;

        //public bool diesInHeat = Settings.plantsDie;
        //public float maxOptimalTemperature = (float)Settings.growth;
        //public float deathTemperature = (float)Settings.survivable;

        private string cachedLabelMouseover;


        // Burnable crops will take damage once the ambient temperature threshold x is reached.
        // This is called once every TickLong (33 seconds?), and happens at a y% chance.
        public override void TickLong()
        {
            base.TickLong();
            HeatDeathCheck();
        }

        private void HeatDeathCheck()
        {
            // The heat death check only passes if death to heat is enabled in the config, and if the temperature is right.
            if (!diesInHeat) return;
            if (this.AmbientTemperature < deathTemperature) return;
            float random = Rand.Value;

            // Each longtick has a 7% chance of killing a plant if the conditions are met.
            if (random < 0.07f)
            {
                // This is a secret that will help us later.
                var map = Map;

                // If the plant isn't the type that can lose its leaves without dying, then we kill it.
                if (def.plant.dieIfLeafless)
                {
                    // Messages should only play for crops!
                    if (this.IsCrop)
                    {
                        if (MessagesRepeatAvoider.MessageShowAllowed("MessagePlantDiedOfHeat-" + def.defName, 240f))
                        {
                            string messageString = "MessagePlantDiedOfHeat".Translate(GetCustomLabelNoCount(false)).CapitalizeFirst();
                            Messages.Message(messageString, new TargetInfo(Position, Map, false), MessageTypeDefOf.NegativeEvent);
                        }
                    }
                    this.TakeDamage(new DamageInfo(DamageDefOf.Rotting, 99999f));
                }

                // Otherwise, we just take the leaves away.
                if (!def.plant.dieIfLeafless)
                {
                    this.madeLeaflessTick = Find.TickManager.TicksGame;
                }

                if (LeaflessNow)
                {
                    map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things);
                }
            }
        }



        // This is required for a workaround. No special behaviour.
        private float GrowthRateFactor_Temperature_Fragile
        {
            get
            {
                float cellTemp;
                if (!GenTemperature.TryGetTemperatureForCell(Position, Map, out cellTemp))
                    return 1;

                return GrowthRateFactorFor_Temperature_Fragile(cellTemp);
            }
        }

        // This is a workaround to lower the temperature range from 43-59 to 38-51 by default.
        private float GrowthRateFactorFor_Temperature_Fragile(float cellTemp)
        {
            if (cellTemp < 6f)
            {
                return Mathf.InverseLerp(0f, 6f, cellTemp);
            }
            if (cellTemp > maxOptimalTemperature)
            {
                // Growth rate decay starts at max optimal temperature and maxes out at the max growth temperature.
                return Mathf.InverseLerp(maxGrowthTemperature, maxOptimalTemperature, cellTemp);
            }
            return 1f;
        }

        //I am not sure if these two are necessary, but I don't want to touch them.
        public override void PostMapInit()
        {
            base.PostMapInit();
            HeatDeathCheck();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (Current.ProgramState == ProgramState.Playing && !respawningAfterLoad)
            {
                PostMapInit();
            }
        }


        // Everything from here on is UI-oriented.


        // GrowthRate affects GrowthPerTick in Plant.cs, as well as some tooltip information.
        public override float GrowthRate
        {
            get
            {
                if (Blighted)
                    return 0f;

                if (Spawned && !PlantUtility.GrowthSeasonNow(Position, Map))
                    return 0f;

                return GrowthRateFactor_Fertility * GrowthRateFactor_Temperature_Fragile * GrowthRateFactor_Light * GrowthRateFactor_NoxiousHaze;
            }
        }
        public override string LabelMouseover
        {
            get
            {
                if (cachedLabelMouseover == null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(def.LabelCap);
                    stringBuilder.Append(" (" + "PercentGrowth".Translate(GrowthPercentString));
                    if (Dying)
                    {
                        stringBuilder.Append(", " + "DyingLower".Translate());
                    }
                    stringBuilder.Append(")");
                    cachedLabelMouseover = stringBuilder.ToString();
                }
                return cachedLabelMouseover;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (LifeStage == PlantLifeStage.Growing)
            {
                stringBuilder.AppendLine("PercentGrowth".Translate(GrowthPercentString));
                stringBuilder.AppendLine("GrowthRate".Translate() + ": " + GrowthRate.ToStringPercent());
                if (!Blighted)
                {
                    if (Resting)
                    {
                        stringBuilder.AppendLine("PlantResting".Translate());
                    }
                    if (!HasEnoughLightToGrow)
                    {
                        stringBuilder.AppendLine("PlantNeedsLightLevel".Translate() + ": " + def.plant.growMinGlow.ToStringPercent());
                    }
                    float growthRateFactor_Temperature = GrowthRateFactor_Temperature_Fragile;
                    if (growthRateFactor_Temperature < 0.99f)
                    {
                        if (growthRateFactor_Temperature < 0.01f)
                        {
                            stringBuilder.AppendLine("OutOfIdealTemperatureRangeNotGrowing".Translate());
                        }
                        else
                        {
                            stringBuilder.AppendLine("OutOfIdealTemperatureRange".Translate(Mathf.RoundToInt(growthRateFactor_Temperature * 100f).ToString()));
                        }
                    }
                }
            }
            else if (LifeStage == PlantLifeStage.Mature)
            {
                if (HarvestableNow)
                {
                    stringBuilder.AppendLine("ReadyToHarvest".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("Mature".Translate());
                }
            }
            if (DyingBecauseExposedToLight)
            {
                stringBuilder.AppendLine("DyingBecauseExposedToLight".Translate());
            }
            if (Blighted)
            {
                stringBuilder.AppendLine("Blighted".Translate() + " (" + Blight.Severity.ToStringPercent() + ")");
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        // This is for the detailed inspection window.
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            //foreach (var stat in base.SpecialDisplayStats())
            //    yield return stat;

            // Growth rate
            if (LifeStage == PlantLifeStage.Growing && Spawned)
            {
                var growthRateDesc = "Stat_Thing_Plant_GrowthRate_Desc".Translate();

                var growthRateCalc = GrowthRateCalcDesc_Fragile;
                if (!growthRateCalc.NullOrEmpty())
                    growthRateDesc += "\n\n" + growthRateCalc;

                growthRateDesc += "\n" + "StatsReport_FinalValue".Translate() + ": " + GrowthRate.ToStringPercent();

                yield return new StatDrawEntry(StatCategoryDefOf.BasicsImportant,
                    "Stat_Thing_Plant_GrowthRate".Translate(),
                    GrowthRate.ToStringPercent(),
                    growthRateDesc,
                    StatDisplayOrder.Thing_Plant_GrowthRate);
            }
        }
        public string GrowthRateCalcDesc_Fragile
        {
            get
            {
                var sb = new StringBuilder();

                if (GrowthRateFactor_Fertility != 1f)
                    sb.AppendInNewLine("StatsReport_MultiplierFor".Translate("FertilityLower".Translate()) + ": " + GrowthRateFactor_Fertility.ToStringPercent());

                if (GrowthRateFactor_Temperature != 1f)
                    sb.AppendInNewLine("StatsReport_MultiplierFor".Translate("TemperatureLower".Translate()) + ": " + GrowthRateFactor_Temperature.ToStringPercent());

                if (GrowthRateFactor_Temperature_Fragile != 1f)
                    sb.AppendInNewLine("StatsReport_MultiplierFor".Translate("TemperatureLower".Translate()) + ": " + GrowthRateFactor_Temperature_Fragile.ToStringPercent());

                if (GrowthRateFactor_Light != 1f)
                    sb.AppendInNewLine("StatsReport_MultiplierFor".Translate("LightLower".Translate()) + ": " + GrowthRateFactor_Light.ToStringPercent());

                if (ModsConfig.BiotechActive && Map.gameConditionManager.ConditionIsActive(GameConditionDefOf.NoxiousHaze) && GrowthRateFactor_NoxiousHaze != 1f)
                    sb.AppendInNewLine("StatsReport_MultiplierFor".Translate(GameConditionDefOf.NoxiousHaze.label) + ": " + GrowthRateFactor_NoxiousHaze.ToStringPercent());

                return sb.ToString();
            }
        }


    }
}
