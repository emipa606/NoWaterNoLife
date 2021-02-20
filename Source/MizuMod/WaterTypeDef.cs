using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class WaterTypeDef : Def
    {
        public float foodPoisonChance = 0.0f;
        public List<HediffDef> hediffs = null;
        public List<ThoughtDef> thoughts = null;
        public WaterPreferability waterPreferability = WaterPreferability.Undefined;
        public WaterType waterType = WaterType.Undefined;
    }
}