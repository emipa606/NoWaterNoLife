using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class CompDestroyByTime : ThingComp
    {
        public CompProperties_DestroyByTime Props => (CompProperties_DestroyByTime)props;

        public int DestroyTicks => Props.destroyTicks;

        private int elapsedTicks = 0;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref elapsedTicks, "elapsedTicks");
        }

        public override void CompTick()
        {
            elapsedTicks += 1;
            CheckTick();
        }

        public override void CompTickRare()
        {
            elapsedTicks += 250;
            CheckTick();
        }

        private void CheckTick()
        {
            if (elapsedTicks >= DestroyTicks)
            {
                var t = parent;

                if (t.holdingOwner != null)
                {
                    t.holdingOwner.Remove(t);
                }
                t.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
