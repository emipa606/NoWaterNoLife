using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_DrawingWaterSpot : Building_WorkTable, IBuilding_DrinkWater
    {
        public bool IsActivated => true;

        public WaterType WaterType => Map.terrainGrid.TerrainAt(Position).ToWaterType();

        public float WaterVolume => 100000f;

        public bool IsEmpty => false;

        public bool CanDrinkFor(Pawn p)
        {
            // 常時使用可能
            return true;
        }

        public bool CanDrawFor(Pawn p)
        {
            // 手が使えればいつでも水汲み可能
            return p.CanManipulate();
        }

        public void DrawWater(float amount)
        {
            // 何もしない
        }
    }
}
