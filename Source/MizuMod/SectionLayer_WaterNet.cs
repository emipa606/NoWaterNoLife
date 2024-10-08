﻿using RimWorld;
using Verse;

namespace MizuMod;

internal class SectionLayer_WaterNet : SectionLayer_Things
{
    public SectionLayer_WaterNet(Section section) : base(section)
    {
        requireAddToMapMesh = false;
        relevantChangeTypes = MapMeshFlagDefOf.Buildings;
    }

    public override void DrawLayer()
    {
        ThingDef thingDef = null;
        switch (Find.DesignatorManager.SelectedDesignator)
        {
            case Designator_Build designator_build:
                thingDef = designator_build.PlacingDef as ThingDef;
                break;
            case Designator_Install designator_install:
                thingDef = designator_install.PlacingDef as ThingDef;
                break;
        }

        if (thingDef != null && typeof(IBuilding_WaterNet).IsAssignableFrom(thingDef.thingClass))
        {
            base.DrawLayer();
        }
    }

    protected override void TakePrintFrom(Thing t)
    {
        if (t is IBuilding_WaterNet building)
        {
            building.PrintForGrid(this);
        }
    }
}