using System;
using Verse;

namespace MizuMod
{
    public class CompProperties_WaterNet : CompProperties
    {
        public CompProperties_WaterNet() : base(typeof(CompWaterNet))
        {
        }

        public CompProperties_WaterNet(Type compClass) : base(compClass)
        {
        }
    }
}