using Verse;

namespace MizuMod;

public class Building_UndergroundDeepWaterPump : Building_UndergroundWaterPump
{
    private MapComponent_WaterGrid waterGrid;

    protected override MapComponent_WaterGrid WaterGrid
    {
        get
        {
            if (waterGrid != null)
            {
                return waterGrid;
            }

            waterGrid = Map.GetComponent<MapComponent_DeepWaterGrid>();
            if (waterGrid == null)
            {
                Log.Error("waterGrid is null");
            }

            return waterGrid;
        }
    }
}