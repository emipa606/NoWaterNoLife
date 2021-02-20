using RimWorld;
using UnityEngine;
using Verse;

namespace MizuMod
{
    public class Designator_AreaSnowGetClear : Designator_AreaSnowGet
    {
        public Designator_AreaSnowGetClear() : base(DesignateMode.Remove)
        {
            defaultLabel = MizuStrings.DesignatorAreaSnowGetClear.Translate();
            defaultDesc = MizuStrings.DesignatorAreaSnowGetClearDescription.Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Designators/SnowClearAreaOff");
            soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_ZoneDelete;
        }
    }
}