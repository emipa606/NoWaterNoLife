using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class PlaceWorker_UndergroundDeepWater : PlaceWorker_UndergroundWater
    {
        private MapComponent_WaterGrid waterGrid;
        public override MapComponent_WaterGrid WaterGrid
        {
            get
            {
                if (waterGrid == null)
                {
                    Map visibleMap = Find.CurrentMap;
                    waterGrid = visibleMap.GetComponent<MapComponent_DeepWaterGrid>();
                }
                return waterGrid;
            }
        }
    }
}
