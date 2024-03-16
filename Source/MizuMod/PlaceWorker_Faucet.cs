using UnityEngine;
using Verse;

namespace MizuMod;

public class PlaceWorker_Faucet : PlaceWorker
{
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var intVecNorth = center + IntVec3.North.RotatedBy(rot);
        GenDraw.DrawFieldEdges([intVecNorth], Color.blue);
    }
}