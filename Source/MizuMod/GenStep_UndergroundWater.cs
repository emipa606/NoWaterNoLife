using RimWorld;
using UnityEngine;
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

        public void GenerateUndergroundWaterGrid(Map map, MapComponent_WaterGrid waterGrid, int basePoolNum,
            int minWaterPoolNum, float baseRainFall, float basePlantDensity, float literPerCell, IntRange poolCellRange,
            FloatRange baseRegenRateRange, float rainRegenRatePerCell)
        {
            var rainRate = map.TileInfo.rainfall / baseRainFall;
            var areaRate = map.Area / BaseMapArea;
            var plantRate = map.Biome.plantDensity / basePlantDensity;

            var waterPoolNum = Mathf.RoundToInt(basePoolNum * rainRate * areaRate * plantRate);

            //Log.Message(string.Format("rain={0},area={1},plant={2},num={3}", rainRate.ToString("F3"), areaRate.ToString("F3"), plantRate.ToString("F3"), waterPoolNum));
            if (plantRate > 0.0f)
            {
                waterPoolNum = Mathf.Max(waterPoolNum, minWaterPoolNum);
            }

            for (var i = 0; i < waterPoolNum; i++)
            {
                if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(5,
                    c => !waterGrid.GetCellBool(map.cellIndices.CellToIndex(c)), map, out var result))
                {
                    continue;
                }

                var numCells = poolCellRange.RandomInRange;
                var baseRegenRate = baseRegenRateRange.RandomInRange;
                var pool = new UndergroundWaterPool(waterGrid, numCells * literPerCell, WaterType.RawWater,
                    baseRegenRate, rainRegenRatePerCell)
                {
                    ID = i + 1
                };
                waterGrid.AddWaterPool(pool, GridShapeMaker.IrregularLump(result, map, numCells));
            }

            waterGrid.ModifyPoolGrid();
        }
    }
}