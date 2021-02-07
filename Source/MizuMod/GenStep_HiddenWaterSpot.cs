using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace MizuMod
{
    public class GenStep_HiddenWaterSpot : GenStep
    {
        public int blockSizeX = 30;
        public int blockSizeZ = 30;
        public int allSpotNum = 100;

        public override void Generate(Map map, GenStepParams parms)
        {
            map.GetComponent<MapComponent_HiddenWaterSpot>().CreateWaterSpot(blockSizeX, blockSizeZ, allSpotNum * map.Area / (250 * 250));
        }

        public override int SeedPart => 38449900;
    }
}
