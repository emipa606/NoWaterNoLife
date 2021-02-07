using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class ThinkNode_ConditionalDehydration : ThinkNode_Conditional
    {
        protected override bool Satisfied(Pawn pawn)
        {
            return pawn.needs.Water() != null && pawn.needs.Water().CurCategory >= ThirstCategory.Dehydration;
        }
    }
}
