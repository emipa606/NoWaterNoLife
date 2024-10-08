﻿using RimWorld;
using UnityEngine;
using Verse;

namespace MizuMod;

public class Graphic_LinkedWaterNet : Graphic_Linked
{
    public Graphic_LinkedWaterNet()
    {
    }

    public Graphic_LinkedWaterNet(Graphic subGraphic)
        : base(subGraphic)
    {
    }

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        return new Graphic_LinkedWaterNet(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo))
        {
            data = data
        };
    }

    public override void Print(SectionLayer layer, Thing parent, float extraRotation)
    {
        Printer_Plane.PrintPlane(
            layer,
            parent.TrueCenter(),
            Vector2.one,
            LinkedDrawMatFrom(parent, parent.Position));
    }

    public override bool ShouldLinkWith(IntVec3 c, Thing parent)
    {
        var thing = parent as IBuilding_WaterNet;

        var foundWaterNetBase = false;
        if (thing == null)
        {
            return false;
        }

        foreach (var net in thing.WaterNetManager.Nets)
        {
            foreach (var t in net.AllThings)
            {
                if (!thing.IsConnectedOr(t, true))
                {
                    continue;
                }

                if (!t.OccupiedRect().Contains(c))
                {
                    continue;
                }

                foundWaterNetBase = true;
                break;
            }

            if (foundWaterNetBase)
            {
                break;
            }
        }

        if (foundWaterNetBase)
        {
            return c.InBounds(parent.Map);
        }

        {
            foreach (var t in thing.WaterNetManager.UnNetThings)
            {
                if (!thing.IsConnectedOr(t, true))
                {
                    continue;
                }

                if (!t.OccupiedRect().Contains(c))
                {
                    continue;
                }

                foundWaterNetBase = true;
                break;
            }
        }

        return c.InBounds(parent.Map) && foundWaterNetBase;
    }
}