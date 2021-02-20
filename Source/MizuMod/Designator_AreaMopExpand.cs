using RimWorld;
using UnityEngine;
using Verse;

namespace MizuMod
{
    public class Designator_AreaMopExpand : Designator_AreaMop
    {
        public Designator_AreaMopExpand() : base(DesignateMode.Add)
        {
            defaultLabel = MizuStrings.DesignatorAreaMopExpand.Translate();
            defaultDesc = MizuStrings.DesignatorAreaMopExpandDescription.Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Designators/HomeAreaOn");
            soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_ZoneAdd;
        }
    }
}