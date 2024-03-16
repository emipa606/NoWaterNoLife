using System;
using System.Collections.Generic;
using Verse;

namespace MizuMod;

public class CompProperties_DestroyMessage(Type compClass) : CompProperties(compClass)
{
    public List<DestroyMode> destroyModes;
    public string messageKey;

    public CompProperties_DestroyMessage() : this(typeof(CompDestroyMessage))
    {
    }
}