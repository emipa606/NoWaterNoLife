﻿using Verse;

namespace MizuMod;

public class PlaceWorker_WaterNet : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(
        BuildableDef checkingDef,
        IntVec3 loc,
        Rot4 rot,
        Map map,
        Thing thingToIgnore = null,
        Thing thing = null)
    {
        var unitX = IntVec3.East.RotatedBy(rot);
        var unitZ = IntVec3.North.RotatedBy(rot);

        for (var z = 0; z < checkingDef.Size.z; z++)
        {
            for (var x = 0; x < checkingDef.Size.x; x++)
            {
                var pos = loc + (unitX * x) + (unitZ * z);
                foreach (var t in map.thingGrid.ThingsAt(pos))
                {
                    if (t is IBuilding_WaterNet)
                    {
                        return new AcceptanceReport(MizuStrings.AcceptanceReportCannotBuildMulti.Translate());
                    }

                    if (t.def.IsBlueprint
                        && typeof(IBuilding_WaterNet).IsAssignableFrom(
                            ((ThingDef)t.def.entityDefToBuild).thingClass))
                    {
                        return new AcceptanceReport(MizuStrings.AcceptanceReportCannotBuildMulti.Translate());
                    }
                }
            }
        }

        return true;
    }
}