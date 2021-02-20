using Verse;

namespace MizuMod
{
    public class GenStep_UndergroundShallowWater : GenStep_UndergroundWater
    {
        public override int SeedPart => 60314899;

        public override void Generate(Map map, GenStepParams parms)
        {
            var waterGrid = map.GetComponent<MapComponent_ShallowWaterGrid>();
            GenerateUndergroundWaterGrid(
                map,
                waterGrid,
                basePoolNum,
                minWaterPoolNum,
                baseRainFall,
                basePlantDensity,
                literPerCell,
                poolCellRange,
                baseRegenRateRange,
                rainRegenRatePerCell);
        }
    }
}