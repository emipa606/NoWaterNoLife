using System;
using System.Collections.Generic;

namespace MizuMod;

public class CompProperties_WaterNetTank(Type compClass) : CompProperties_WaterNet(compClass)
{
    public enum DrawType : byte
    {
        Undefined = 0,

        Self,

        Faucet
    }

    public readonly List<DrawType> drawTypes = [DrawType.Faucet];

    public readonly int flatID = -1;

    public readonly float maxWaterVolume = 0f;

    public readonly bool showBar = true;

    public CompProperties_WaterNetTank()
        : this(typeof(CompWaterNetTank))
    {
    }
}