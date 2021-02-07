using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public abstract class Building_UndergroundWaterPump : Building_WaterNet, IBuilding_WaterNet
    {
        public abstract MapComponent_WaterGrid WaterGrid { get; }
        private UndergroundWaterPool pool = null;

        public override WaterType OutputWaterType => pool.WaterType;

        public override UndergroundWaterPool WaterPool => pool;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            pool = WaterGrid.GetPool(map.cellIndices.CellToIndex(Position));
            if (pool == null)
            {
                Log.Error("pool is null");
            }
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (stringBuilder.ToString() != string.Empty)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append(string.Format(MizuStrings.InspectStoredWaterPool.Translate() + ": {0}%", (pool.CurrentWaterVolumePercent * 100).ToString("F0")));
            if (DebugSettings.godMode)
            {
                stringBuilder.Append(string.Format(" ({0}/{1} L)", pool.CurrentWaterVolume.ToString("F2"), pool.MaxWaterVolume.ToString("F2")));
            }

            return stringBuilder.ToString();
        }
    }
}
