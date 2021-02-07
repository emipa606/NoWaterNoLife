using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    internal class SectionLayer_WaterNet : SectionLayer_Things
    {
        public SectionLayer_WaterNet(Section section) : base(section)
		{
            requireAddToMapMesh = false;
            relevantChangeTypes = MapMeshFlag.Buildings;
        }

        public override void DrawLayer()
        {
            ThingDef thingDef = null;
            if (Find.DesignatorManager.SelectedDesignator is Designator_Build designator_build)
            {
                thingDef = designator_build.PlacingDef as ThingDef;
            }
            if (Find.DesignatorManager.SelectedDesignator is Designator_Install designator_install)
            {
                thingDef = designator_install.PlacingDef as ThingDef;
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
}
