using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MizuMod
{
    public class PlaceWorker_Valve : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            var intVecSouth = center + IntVec3.South.RotatedBy(rot);
            var intVecNorth = center + IntVec3.North.RotatedBy(rot);
            GenDraw.DrawFieldEdges(new List<IntVec3> {intVecSouth}, Color.blue);
            GenDraw.DrawFieldEdges(new List<IntVec3> {intVecNorth}, Color.blue);
        }
    }
}