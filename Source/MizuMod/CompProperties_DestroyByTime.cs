using System;
using Verse;

namespace MizuMod;

public class CompProperties_DestroyByTime : CompProperties
{
    public int destroyTicks = 1;

    public CompProperties_DestroyByTime() : base(typeof(CompDestroyByTime))
    {
    }

    public CompProperties_DestroyByTime(Type compClass) : base(compClass)
    {
    }
}