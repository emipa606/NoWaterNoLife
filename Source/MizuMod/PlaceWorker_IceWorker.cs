﻿using Verse;

namespace MizuMod;

public class PlaceWorker_IceWorker : PlaceWorker
{
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

        var cond_building = false;
        var terrainLoc = map.terrainGrid.TerrainAt(loc);
        if (terrainLoc.IsIce())
        {
            cond_building = true;
        }

        var cond_interaction = true;
        if (!def.hasInteractionCell)
        {
            return cond_building;
        }

        var terrainInteraction = map.terrainGrid.TerrainAt(ThingUtility.InteractionCellWhenAt(def, loc, rot, map));
        if (terrainInteraction.passability != Traversability.Standable)
        {
            cond_interaction = false;
        }

        return cond_building && cond_interaction;
    }
}