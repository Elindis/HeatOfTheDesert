using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using System.ComponentModel;

namespace HeatOfTheDesert
{ 
    public class HeatOfTheDesertExtension : DefModExtension
    {
        //static HeatOfTheDesertExtension()
        //{
        //    Log.Message("Mod initialized!");
        //}

        public bool diesInHeat = true;
        public float deathTemperature = 48f;
        public float maxOptimalTemperature = 42f;
    }
}
