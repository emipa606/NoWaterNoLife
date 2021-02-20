using System;
using System.Collections.Generic;

namespace MizuMod
{
    public class CompProperties_WaterNetInput : CompProperties_WaterNet
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

        public List<WaterType> acceptWaterTypes = new List<WaterType>
        {
            WaterType.ClearWater,
            WaterType.NormalWater,
            WaterType.RawWater,
            WaterType.MudWater,
            WaterType.SeaWater
        };

        public float baseRainFlow = 1000f;

        //public InputType inputType = InputType.WaterNet;
        public List<InputType> inputTypes = new List<InputType>
        {
            InputType.WaterNet
        };

        public InputWaterFlowType inputWaterFlowType = InputWaterFlowType.Any;

        public float maxInputWaterFlow = float.MaxValue;
        public int roofDistance = 1;
        public float roofEfficiency = 0.5f;

        public CompProperties_WaterNetInput() : base(typeof(CompWaterNetInput))
        {
        }

        public CompProperties_WaterNetInput(Type compClass) : base(compClass)
        {
        }
    }
}