using RimWorld;
using UnityEngine;
using Verse;

namespace MizuMod
{
    public class Designator_AreaMopClear : Designator_AreaMop
    {
        public Designator_AreaMopClear() : base(DesignateMode.Remove)
        {
            defaultLabel = MizuStrings.DesignatorAreaMopClear.Translate();
            defaultDesc = MizuStrings.DesignatorAreaMopClearDescription.Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Designators/HomeAreaOff");
            soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_ZoneDelete;
        }
    }
}