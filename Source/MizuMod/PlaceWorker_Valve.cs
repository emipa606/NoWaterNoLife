using UnityEngine;
using Verse;

namespace MizuMod;

public class PlaceWorker_Valve : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var intVecSouth = center + IntVec3.South.RotatedBy(rot);
        var intVecNorth = center + IntVec3.North.RotatedBy(rot);
        if (def.size == IntVec2.One)
        {
            GenDraw.DrawFieldEdges([intVecSouth], Color.blue);
            GenDraw.DrawFieldEdges([intVecNorth], Color.blue);
            return;
        }

        if (def.size != IntVec2.Two)
        {
            return;
        }

        GenDraw.DrawFieldEdges([intVecSouth], Color.blue);
        GenDraw.DrawFieldEdges([intVecSouth + IntVec3.East.RotatedBy(rot)], Color.blue);
        GenDraw.DrawFieldEdges([intVecNorth + IntVec3.North.RotatedBy(rot)], Color.blue);
        GenDraw.DrawFieldEdges(
            [intVecNorth + IntVec3.North.RotatedBy(rot) + IntVec3.East.RotatedBy(rot)], Color.blue);
    }
}