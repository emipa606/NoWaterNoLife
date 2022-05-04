using System;
using System.Collections.Generic;
using Verse;

namespace MizuMod;

public class CompProperties_WaterTool : CompProperties
{
    public enum UseWorkType : byte
    {
        Undefined = 0,

        Mop,

        Nurse,

        WaterFarm,

        FightFire
    }

    public float maxWaterVolume = 1f;

    public List<WorkTypeDef> supplyWorkType = new List<WorkTypeDef>();

    public List<UseWorkType> useWorkType = new List<UseWorkType>();

    public CompProperties_WaterTool()
        : base(typeof(CompWaterTool))
    {
    }

    public CompProperties_WaterTool(Type compClass)
        : base(compClass)
    {
    }
}