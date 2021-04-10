using UnityEngine;
using Verse;

namespace MizuMod
{
    public class PlaceWorker_WaterWorker : PlaceWorker
    {
        // デバッグ用
        private MapComponent_HiddenWaterSpot HiddenWaterSpot => Find.CurrentMap.GetComponent<MapComponent_HiddenWaterSpot>();

        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (!(checkingDef is ThingDef))
            {
                Log.Error("invalid ThingDef");
                return false;
            }

            var terrainLoc = map.terrainGrid.TerrainAt(loc);
            if (!(terrainLoc.IsSea() || terrainLoc.IsRiver() || terrainLoc.IsLakeOrPond() || terrainLoc.IsMarsh()))
            {
                // 水でないなら
                return new AcceptanceReport(MizuStrings.AcceptanceReportCantBuildExceptOverWater.Translate());
            }

            return true;
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            base.DrawGhost(def, center, rot, ghostCol, thing);

            if (DebugSettings.godMode)
            {
                // デバッグ用
                HiddenWaterSpot.MarkForDraw();
            }
        }
    }
}