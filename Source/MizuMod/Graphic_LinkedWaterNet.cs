using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace MizuMod
{
    public class Graphic_LinkedWaterNet : Graphic_Linked
    {
        public Graphic_LinkedWaterNet() : base()
        {
        }

        public Graphic_LinkedWaterNet(Graphic subGraphic) : base(subGraphic)
        {
        }

        public override bool ShouldLinkWith(IntVec3 c, Thing parent)
        {
            var thing = parent as IBuilding_WaterNet;

            var foundWaterNetBase = false;
            foreach (var net in thing.WaterNetManager.Nets)
            {
                foreach (var t in net.AllThings)
                {
                    if (thing.IsConnectedOr(t, true))
                    {
                        if (t.OccupiedRect().Contains(c))
                        {
                            foundWaterNetBase = true;
                            break;
                        }
                    }
                }
                if (foundWaterNetBase)
                {
                    break;
                }
            }

            if (!foundWaterNetBase)
            {
                foreach (var t in thing.WaterNetManager.UnNetThings)
                {
                    if (thing.IsConnectedOr(t, true))
                    {
                        if (t.OccupiedRect().Contains(c))
                        {
                            foundWaterNetBase = true;
                            break;
                        }
                    }
                }
            }

            return GenGrid.InBounds(c, parent.Map) && foundWaterNetBase;
        }

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return new Graphic_LinkedWaterNet(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo))
            {
                data = data
            };
        }

        public override void Print(SectionLayer layer, Thing parent)
        {
            Printer_Plane.PrintPlane(layer, parent.TrueCenter(), Vector2.one, LinkedDrawMatFrom(parent, parent.Position), 0f, false, null, null, 0.01f);
        }
    }
}
