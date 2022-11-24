using UnityEngine;
using Verse;

namespace MizuMod;

public abstract class PlaceWorker_UndergroundWater : PlaceWorker
{
    public abstract MapComponent_WaterGrid WaterGrid { get; }

    public override AcceptanceReport AllowsPlacing(
        BuildableDef checkingDef,
        IntVec3 loc,
        Rot4 rot,
        Map map,
        Thing thingToIgnore = null,
        Thing thing = null)
    {
        if (checkingDef is not ThingDef def)
        {
            return false;
        }

        var curID = 0;

        for (var x = 0; x < def.Size.x; x++)
        {
            for (var z = 0; z < def.Size.z; z++)
            {
                var relVec = new IntVec3(x, 0, z).RotatedBy(rot);
                var curVec = loc + relVec;

                var poolID = WaterGrid.GetID(map.cellIndices.CellToIndex(curVec));
                if (poolID == 0)
                {
                    return false;
                }

                if (curID == 0)
                {
                    curID = poolID;
                }
                else if (curID != poolID)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        WaterGrid.MarkForDraw();
    }
}