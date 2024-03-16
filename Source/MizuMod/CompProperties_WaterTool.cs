using System;
using System.Collections.Generic;
using Verse;

namespace MizuMod;

public class CompProperties_WaterTool(Type compClass) : CompProperties(compClass)
{
    public enum UseWorkType : byte
    {
        Undefined = 0,

        Mop,

        Nurse,

        WaterFarm,

        FightFire
    }

    public readonly float maxWaterVolume = 1f;

    public readonly List<WorkTypeDef> supplyWorkType = [];

    public readonly List<UseWorkType> useWorkType = [];

    public CompProperties_WaterTool()
        : this(typeof(CompWaterTool))
    {
    }
}