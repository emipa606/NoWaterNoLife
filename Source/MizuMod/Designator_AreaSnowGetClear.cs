using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;

namespace MizuMod
{
    public class Designator_AreaSnowGetClear : Designator_AreaSnowGet
    {
        public Designator_AreaSnowGetClear() : base(DesignateMode.Remove)
        {
            defaultLabel = MizuStrings.DesignatorAreaSnowGetClear.Translate();
            defaultDesc = MizuStrings.DesignatorAreaSnowGetClearDescription.Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Designators/SnowClearAreaOff", true);
            soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_ZoneDelete;
        }
    }
}
