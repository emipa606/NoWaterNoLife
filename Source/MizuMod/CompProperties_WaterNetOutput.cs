using System;

namespace MizuMod
{
    public class CompProperties_WaterNetOutput : CompProperties_WaterNet
    {
        public enum OutputWaterFlowType : byte
        {
            Undefined = 0,

            Constant,

            Any
        }

        public WaterType forceOutputWaterType = WaterType.Undefined;

        public float maxOutputWaterFlow = 0.0f;

        public OutputWaterFlowType outputWaterFlowType = OutputWaterFlowType.Constant;

        public CompProperties_WaterNetOutput()
            : base(typeof(CompWaterNetOutput))
        {
        }

        public CompProperties_WaterNetOutput(Type compClass)
            : base(compClass)
        {
        }
    }
}