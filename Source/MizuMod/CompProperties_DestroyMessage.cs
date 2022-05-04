using System;
using System.Collections.Generic;
using Verse;

namespace MizuMod;

public class CompProperties_DestroyMessage : CompProperties
{
    public List<DestroyMode> destroyModes;
    public string messageKey;

    public CompProperties_DestroyMessage() : base(typeof(CompDestroyMessage))
    {
    }

    public CompProperties_DestroyMessage(Type compClass) : base(compClass)
    {
    }
}