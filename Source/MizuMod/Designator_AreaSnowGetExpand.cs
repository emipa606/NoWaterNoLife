using RimWorld;
using UnityEngine;
using Verse;

namespace MizuMod;

public class Designator_AreaSnowGetExpand : Designator_AreaSnowGet
{
    public Designator_AreaSnowGetExpand() : base(DesignateMode.Add)
    {
        defaultLabel = MizuStrings.DesignatorAreaSnowGetExpand.Translate();
        defaultDesc = MizuStrings.DesignatorAreaSnowGetExpandDescription.Translate();
        icon = ContentFinder<Texture2D>.Get("UI/Designators/SnowClearAreaOn");
        soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
        soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
        soundSucceeded = SoundDefOf.Designate_ZoneAdd;
    }
}