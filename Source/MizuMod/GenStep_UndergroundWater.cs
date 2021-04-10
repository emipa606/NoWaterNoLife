using Verse;

namespace MizuMod
{
    public abstract class GenStep_UndergroundWater : GenStep
    {
        private const float BaseMapArea = 250f * 250f;

        protected readonly float basePlantDensity = 0.25f;

        protected readonly int basePoolNum = 30;

        protected readonly float baseRainFall = 1000f;

        protected readonly float literPerCell = 10.0f;

        protected readonly int minWaterPoolNum = 3;

        protected FloatRange baseRegenRateRange = new FloatRange(10.0f, 20.0f);

        protected IntRange poolCellRange = new IntRange(30, 100);

        protected float rainRegenRatePerCell = 5.0f;
    }
}