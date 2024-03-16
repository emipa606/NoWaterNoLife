using RimWorld;
using UnityEngine;
using Verse;

namespace MizuMod;

public class Designator_DeconstructPipe : Designator_Deconstruct
{
    public Designator_DeconstructPipe()
    {
        defaultLabel = MizuStrings.DesignatorDeconstructPipe.Translate();
        defaultDesc = MizuStrings.DesignatorDeconstructPipeDescription.Translate();
        icon = ContentFinder<Texture2D>.Get("UI/Designators/Deconstruct");
        soundDragSustain = SoundDefOf.Designate_DragStandard;
        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        useMouseIcon = true;
        soundSucceeded = SoundDefOf.Designate_Deconstruct;

        // this.hotKey = KeyBindingDefOf.DesignatorDeconstruct;
    }

    public override AcceptanceReport CanDesignateThing(Thing t)
    {
        // 建設済みのパイプなら〇
        if (base.CanDesignateThing(t).Accepted && t is Building_Pipe)
        {
            return true;
        }

        // パイプの設計or施行なら〇
        return (t.def.IsBlueprint || t.def.IsFrame) && (t.def.entityDefToBuild == MizuDef.Thing_WaterPipe
                                                        || t.def.entityDefToBuild == MizuDef.Thing_WaterPipeInWater);
        // それ以外は×
    }

    public override void DesignateThing(Thing t)
    {
        if (t.def.Claimable && t.Faction != Faction.OfPlayer)
        {
            t.SetFaction(Faction.OfPlayer);
        }

        var innerIfMinified = t.GetInnerIfMinified();
        if (DebugSettings.godMode || innerIfMinified.GetStatValue(StatDefOf.WorkToBuild) == 0f || t.def.IsFrame
            || t.def.IsBlueprint)
        {
            t.Destroy(DestroyMode.Deconstruct);
        }
        else
        {
            Map.designationManager.AddDesignation(new Designation(t, DesignationDefOf.Deconstruct));
        }
    }
}