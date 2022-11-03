using Verse;

namespace HeatOfTheDesert
{
    public class HeatOfTheDesertExtension : DefModExtension
    {
        //static HeatOfTheDesertExtension()
        //{
        //    Log.Message("Mod initialized!");
        //}

        public bool diesInHeat = Settings.plantsDie;
        public float deathTemperature = Settings.survivable;
        public float maxOptimalTemperature = Settings.optimal;
        public float maxGrowthTemperature = Settings.growth;
    }
}
