using System;
using Verse;

namespace MizuMod;

public class CompProperties_DestroyByTime(Type compClass) : CompProperties(compClass)
{
    public readonly int destroyTicks = 1;

    public CompProperties_DestroyByTime() : this(typeof(CompDestroyByTime))
    {
    }
}