using Verse;

namespace MizuMod
{
    public class GSForDebug
    {
        public WaterType changeWaterPoolType = WaterType.NoWater;

        public bool enableAlwaysActivateSprinklerGrowing = false;

        public bool enableChangeWaterPoolType = false;

        public bool enableChangeWaterPoolVolume = false;

        public bool enableResetHiddenWaterSpot = false;

        public bool enableResetRegenRate = false;

        public float latentHeatRate = 1.0f;

        public float needWaterReduceRate = 1.0f;

        public FloatRange resetBaseRegenRateRangeForDeep = new FloatRange(40.0f, 80.0f);

        public FloatRange resetBaseRegenRateRangeForShallow = new FloatRange(10.0f, 20.0f);

        public int resetHiddenWaterSpotAllSpotNum = 100;

        public int resetHiddenWaterSpotBlockSizeX = 30;

        public int resetHiddenWaterSpotBlockSizeZ = 30;

        public float resetRainRegenRatePerCellForDeep = 5.0f;

        public float resetRainRegenRatePerCellForShallow = 10.0f;

        public float waterPoolVolumeRate = 1.0f;
    }
}