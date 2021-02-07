using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class GenStep_UndergroundShallowWater : GenStep_UndergroundWater
    {
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

        public override int SeedPart => 60314899;

    }
}
