using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace MizuMod
{
    public class Area_Mop : Area
    {
        public override string Label => MizuStrings.AreaMop.Translate();

        public override Color Color => new Color(0.3f, 0.3f, 0.9f);

        public override int ListPriority => 3999;

        public Area_Mop()
        {
        }

        public Area_Mop(AreaManager areaManager) : base(areaManager)
        {
        }

        public override string GetUniqueLoadID()
        {
            return "Area_" + ID + "_MizuMop";
        }
    }
}
