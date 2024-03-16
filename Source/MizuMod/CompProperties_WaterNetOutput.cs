using System;

namespace MizuMod;

public class CompProperties_WaterNetOutput(Type compClass) : CompProperties_WaterNet(compClass)
{
    public enum OutputWaterFlowType : byte
    {
        Undefined = 0,

        Constant,

        Any
    }

    public readonly WaterType forceOutputWaterType = WaterType.Undefined;

    public readonly float maxOutputWaterFlow = 0.0f;

    public readonly OutputWaterFlowType outputWaterFlowType = OutputWaterFlowType.Constant;

    public CompProperties_WaterNetOutput()
        : this(typeof(CompWaterNetOutput))
    {
    }
}