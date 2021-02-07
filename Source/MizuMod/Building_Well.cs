﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_Well : Building_WorkTable, IBuilding_DrinkWater
    {
        private UndergroundWaterPool pool = null;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            var waterGrid = map.GetComponent<MapComponent_ShallowWaterGrid>();
            if (waterGrid == null)
            {
                Log.Error("waterGrid is null");
            }

            pool = waterGrid.GetPool(map.cellIndices.CellToIndex(Position));
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

        public bool IsActivated => true;

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

        public bool CanDrinkFor(Pawn p)
        {
            if (p.needs == null || p.needs.Water() == null)
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
            return p.CanManipulate() && pool.CurrentWaterVolume >= p.needs.Water().WaterWanted * Need_Water.DrinkFromBuildingMargin;
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

            var waterItemDef = MizuDef.List_WaterItem.First((def) => def.GetCompProperties<CompProperties_WaterSource>().waterType == pool.WaterType);
            var compprop = waterItemDef.GetCompProperties<CompProperties_WaterSource>();

            // 汲める予定の水アイテムの水の量より多い
            return p.CanManipulate() && pool.CurrentWaterVolume >= compprop.waterVolume;
        }

        public void DrawWater(float amount)
        {
            if (pool == null)
            {
                return;
            }

            pool.CurrentWaterVolume = Mathf.Max(pool.CurrentWaterVolume - amount, 0);
        }
    }
}
