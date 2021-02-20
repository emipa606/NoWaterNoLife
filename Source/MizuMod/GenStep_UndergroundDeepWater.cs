using Verse;

namespace MizuMod
{
    public class GenStep_UndergroundDeepWater : GenStep_UndergroundWater
    {
        public override int SeedPart => 51037366;

        public override void Generate(Map map, GenStepParams parms)
        {
            var waterGrid = map.GetComponent<MapComponent_DeepWaterGrid>();
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