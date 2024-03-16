using System;
using System.Collections.Generic;

namespace MizuMod;

public class CompProperties_WaterNetInput(Type compClass) : CompProperties_WaterNet(compClass)
{
    public enum InputType : byte
    {
        Undefined = 0,

        WaterNet,

        Rain,

        WaterPool,

        Terrain
    }

    public enum InputWaterFlowType : byte
    {
        Undefined = 0,

        Constant,

        Any
    }

    public readonly List<WaterType> acceptWaterTypes =
    [
        WaterType.ClearWater,
        WaterType.NormalWater,
        WaterType.RawWater,
        WaterType.MudWater,
        WaterType.SeaWater
    ];

    public readonly float baseRainFlow = 1000f;

    // public InputType inputType = InputType.WaterNet;
    public readonly List<InputType> inputTypes = [InputType.WaterNet];

    public readonly InputWaterFlowType inputWaterFlowType = InputWaterFlowType.Any;

    public readonly float maxInputWaterFlow = float.MaxValue;

    public readonly int roofDistance = 1;

    public readonly float roofEfficiency = 0.5f;

    public CompProperties_WaterNetInput()
        : this(typeof(CompWaterNetInput))
    {
    }
}