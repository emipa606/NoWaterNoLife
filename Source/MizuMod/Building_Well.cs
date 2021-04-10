using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace MizuMod
{
    public class Building_Well : Building_WorkTable, IBuilding_DrinkWater
    {
        private UndergroundWaterPool pool;

        public bool IsActivated => true;

        public bool IsEmpty
        {
            get
            {
                if (pool == null)
                {
                    return true;
                }

                if (pool.CurrentWaterVolume <= 0f)
                {
                    return true;
                }

                return false;
            }
        }

        public WaterType WaterType
        {
            get
            {
                if (pool == null)
                {
                    return WaterType.Undefined;
                }

                return pool.WaterType;
            }
        }

        public float WaterVolume
        {
            get
            {
                if (pool == null)
                {
                    return 0f;
                }

                return pool.CurrentWaterVolume;
            }
        }

        public bool CanDrawFor(Pawn p)
        {
            if (pool == null)
            {
                return false;
            }

            if (pool.WaterType == WaterType.Undefined || pool.WaterType == WaterType.NoWater)
            {
                return false;
            }

            var waterItemDef = MizuDef.List_WaterItem.First(
                thingDef => thingDef.GetCompProperties<CompProperties_WaterSource>().waterType == pool.WaterType);
            var compprop = waterItemDef.GetCompProperties<CompProperties_WaterSource>();

            // 汲める予定の水アイテムの水の量より多い
            return p.CanManipulate() && pool.CurrentWaterVolume >= compprop.waterVolume;
        }

        public bool CanDrinkFor(Pawn p)
        {
            if (p.needs?.Water() == null)
            {
                return false;
            }

            if (pool == null)
            {
                return false;
            }

            if (pool.WaterType == WaterType.Undefined || pool.WaterType == WaterType.NoWater)
            {
                return false;
            }

            // 手が使用可能で、地下水の水量が十分にある
            return p.CanManipulate() && pool.CurrentWaterVolume
                   >= p.needs.Water().WaterWanted * Need_Water.DrinkFromBuildingMargin;
        }

        public void DrawWater(float amount)
        {
            if (pool == null)
            {
                return;
            }

            pool.CurrentWaterVolume = Mathf.Max(pool.CurrentWaterVolume - amount, 0);
        }

        public override string GetInspectString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetInspectString());

            if (stringBuilder.ToString() != string.Empty)
            {
                stringBuilder.AppendLine();
            }

            stringBuilder.Append(
                string.Format(
                    MizuStrings.InspectStoredWaterPool.Translate() + ": {0}%",
                    (pool.CurrentWaterVolumePercent * 100).ToString("F0")));
            if (DebugSettings.godMode)
            {
                stringBuilder.Append($" ({pool.CurrentWaterVolume:F2}/{pool.MaxWaterVolume:F2} L)");
            }

            return stringBuilder.ToString();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            var waterGrid = map.GetComponent<MapComponent_ShallowWaterGrid>();
            if (waterGrid == null)
            {
                Log.Error("waterGrid is null");
            }

            if (waterGrid != null)
            {
                pool = waterGrid.GetPool(map.cellIndices.CellToIndex(Position));
            }

            if (pool == null)
            {
                Log.Error("pool is null");
            }
        }
    }
}