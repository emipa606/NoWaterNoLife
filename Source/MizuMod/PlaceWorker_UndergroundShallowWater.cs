using Verse;

namespace MizuMod;

public class PlaceWorker_UndergroundShallowWater : PlaceWorker_UndergroundWater
{
    private MapComponent_WaterGrid waterGrid;

    public override MapComponent_WaterGrid WaterGrid
    {
        get
        {
            if (waterGrid != null)
            {
                return waterGrid;
            }

            var visibleMap = Find.CurrentMap;
            waterGrid = visibleMap.GetComponent<MapComponent_ShallowWaterGrid>();

            return waterGrid;
        }
    }
}