using UnityEngine;
using Verse;

namespace MizuMod
{
    public class Graphic_LinkedWaterNetOverlay : Graphic_Linked
    {
        public Graphic_LinkedWaterNetOverlay()
        {
        }

        public Graphic_LinkedWaterNetOverlay(Graphic subGraphic)
            : base(subGraphic)
        {
        }

        public override void Print(SectionLayer layer, Thing parent, float extraRotation)
        {
            foreach (var current in parent.OccupiedRect())
            {
                var vector = current.ToVector3ShiftedWithAltitude(AltitudeLayer.MapDataOverlay);
                Printer_Plane.PrintPlane(layer, vector, Vector2.one, LinkedDrawMatFrom(parent, current));
            }
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            var isFound = false;
            if (!(parent is IBuilding_WaterNet thing))
            {
                return false;
            }

            if (parent.OccupiedRect().Contains(c))
            {
                return c.InBounds(parent.Map);
            }

            if (!thing.HasConnector)
            {
                return false;
            }

            foreach (var net in thing.WaterNetManager.Nets)
            {
                foreach (var t in net.AllThings)
                {
                    if (t == thing)
                    {
                        continue;
                    }

                    if (!t.HasConnector)
                    {
                        continue;
                    }

                    if (t.IsConnectedOr(thing) && t.OccupiedRect().Contains(c))
                    {
                        isFound = true;
                    }
                }

                if (isFound)
                {
                    break;
                }
            }

            return c.InBounds(parent.Map) && isFound;
        }
    }
}