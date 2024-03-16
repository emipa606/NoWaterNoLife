using Verse;

namespace MizuMod;

public class GSForDebug
{
    public readonly WaterType changeWaterPoolType = WaterType.NoWater;

    public readonly bool enableAlwaysActivateSprinklerGrowing = false;

    public readonly bool enableChangeWaterPoolType = false;

    public readonly bool enableChangeWaterPoolVolume = false;

    public readonly bool enableResetHiddenWaterSpot = false;

    public readonly bool enableResetRegenRate = false;

    public readonly float latentHeatRate = 1.0f;

    public readonly float needWaterReduceRate = 1.0f;

    public readonly int resetHiddenWaterSpotAllSpotNum = 100;

    public readonly int resetHiddenWaterSpotBlockSizeX = 30;

    public readonly int resetHiddenWaterSpotBlockSizeZ = 30;

    public readonly float resetRainRegenRatePerCellForDeep = 5.0f;

    public readonly float resetRainRegenRatePerCellForShallow = 10.0f;

    public readonly float waterPoolVolumeRate = 1.0f;

    public FloatRange resetBaseRegenRateRangeForDeep = new FloatRange(40.0f, 80.0f);

    public FloatRange resetBaseRegenRateRangeForShallow = new FloatRange(10.0f, 20.0f);
}