using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MizuMod;

public class WaterTypeDef : Def
{
    public readonly float foodPoisonChance = 0.0f;

    public readonly List<HediffDef> hediffs = null;

    public readonly List<ThoughtDef> thoughts = null;

    public readonly WaterPreferability waterPreferability = WaterPreferability.Undefined;

    public WaterType waterType = WaterType.Undefined;
}