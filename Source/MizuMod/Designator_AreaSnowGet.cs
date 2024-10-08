﻿using RimWorld;
using Verse;

namespace MizuMod;

public abstract class Designator_AreaSnowGet : Designator
{
    private readonly DesignateMode mode;

    public Designator_AreaSnowGet(DesignateMode mode)
    {
        this.mode = mode;
        soundDragSustain = SoundDefOf.Designate_DragStandard;
        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        useMouseIcon = true;

        // this.hotKey = KeyBindingDefOf.Misc7;
        // this.tutorTag = "AreaSnowClear";
    }

    public override bool DragDrawMeasurements => true;

    public override int DraggableDimensions => 2;

    public override AcceptanceReport CanDesignateCell(IntVec3 c)
    {
        if (!c.InBounds(Map))
        {
            return false;
        }

        if (mode == DesignateMode.Add)
        {
            return !Map.areaManager.SnowGet()[c];
        }

        return Map.areaManager.SnowGet()[c];
    }

    public override void DesignateSingleCell(IntVec3 c)
    {
        if (mode == DesignateMode.Add)
        {
            Map.areaManager.SnowGet()[c] = true;
        }
        else
        {
            Map.areaManager.SnowGet()[c] = false;
        }
    }

    public override void SelectedUpdate()
    {
        GenUI.RenderMouseoverBracket();
        Map.areaManager.SnowGet().MarkForDraw();
    }
}