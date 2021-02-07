using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;

namespace MizuMod
{
    public class Designator_AreaSnowGetExpand : Designator_AreaSnowGet
    {
        public Designator_AreaSnowGetExpand() : base(DesignateMode.Add)
        {
            defaultLabel = MizuStrings.DesignatorAreaSnowGetExpand.Translate();
            defaultDesc = MizuStrings.DesignatorAreaSnowGetExpandDescription.Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Designators/SnowClearAreaOn", true);
            soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_ZoneAdd;
        }
    }
}
