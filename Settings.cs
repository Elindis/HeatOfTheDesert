using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace HeatOfTheDesert
{
    public class Settings : ModSettings
    {
        /// <summary>
        /// The three settings our mod has.
        /// </summary>
        public static bool plantsDie;
        public static int optimal = 38;
        public static int survivable = 46;
        public static int growth = 52;

        /// <summary>
        /// The part that writes our settings to file. Note that saving is by ref.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref plantsDie, "doPlantsDie");
            Scribe_Values.Look(ref optimal, "optimalTemperature", 43);
            Scribe_Values.Look(ref survivable, "survivableTemperature", 50);
            Scribe_Values.Look(ref growth, "growthTemperature", 50);
            base.ExposeData();
        }
    }

    public class ExampleMod : Mod
    {
        /// <summary>
        /// A reference to our settings.
        /// </summary>
        Settings settings;

        /// <summary>
        /// A mandatory constructor which resolves the reference to our settings.
        /// </summary>
        /// <param name="content"></param>
        public ExampleMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<Settings>();
        }
        public override void WriteSettings()
        {
            base.WriteSettings();
            settings.ExposeData();
        }

        /// <summary>
        /// The (optional) GUI part to set your settings.
        /// </summary>
        /// <param name="inRect">A Unity Rect with the size of the settings window.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            listingStandard.ColumnWidth /= 2;

            listingStandard.CheckboxLabeled("Plants die in heat", ref Settings.plantsDie);

            listingStandard.Label("");
            listingStandard.Label("Plant settings:");
            listingStandard.Label("");

            listingStandard.Label("Max optimal temperature: " +Mathf.Round(Settings.optimal));
            listingStandard.IntAdjuster(ref Settings.optimal, 1, 0);
            listingStandard.IntSetter(ref Settings.optimal, 38, "Default", 30f);

            listingStandard.Label("");

            listingStandard.Label("Max growth temperature: "+Mathf.Round(Settings.growth));
            listingStandard.IntAdjuster(ref Settings.growth, 1, 0);
            listingStandard.IntSetter(ref Settings.growth, 46, "Default", 30f);

            listingStandard.Label("");

            listingStandard.Label("Max survivable temperature: " + Mathf.Round(Settings.survivable));
            listingStandard.IntAdjuster(ref Settings.survivable, 1, 0);
            listingStandard.IntSetter(ref Settings.survivable, 52, "Default", 30f);

            listingStandard.Label("");
            listingStandard.Label("Note: Restart required for changes to load.");

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        /// <summary>
        /// Override SettingsCategory to show up in the list of settings.
        /// Using .Translate() is optional, but does allow for localisation.
        /// </summary>
        /// <returns>The (translated) mod name.</returns>
        public override string SettingsCategory()
        {
            return "HeatOfTheDesert".Translate();
        }
    }
}