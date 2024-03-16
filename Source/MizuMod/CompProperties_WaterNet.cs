using System;
using Verse;

namespace MizuMod;

public class CompProperties_WaterNet(Type compClass) : CompProperties(compClass)
{
    public CompProperties_WaterNet() : this(typeof(CompWaterNet))
    {
    }
}