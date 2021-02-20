using Verse;

namespace MizuMod
{
    public class Building_UndergroundShallowWaterPump : Building_UndergroundWaterPump
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

                waterGrid = Map.GetComponent<MapComponent_ShallowWaterGrid>();
                if (waterGrid == null)
                {
                    Log.Error("waterGrid is null");
                }

                return waterGrid;
            }
        }
    }
}