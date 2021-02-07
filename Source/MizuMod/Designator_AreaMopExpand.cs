using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;

namespace MizuMod
{
    public class Designator_AreaMopExpand : Designator_AreaMop
    {
        public Designator_AreaMopExpand() : base(DesignateMode.Add)
        {
            defaultLabel = MizuStrings.DesignatorAreaMopExpand.Translate();
            defaultDesc = MizuStrings.DesignatorAreaMopExpandDescription.Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Designators/HomeAreaOn", true);
            soundDragSustain = SoundDefOf.Designate_DragAreaAdd;
            soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            soundSucceeded = SoundDefOf.Designate_ZoneAdd;
        }
    }
}
