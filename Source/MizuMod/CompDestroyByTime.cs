using Verse;

namespace MizuMod
{
    public class CompDestroyByTime : ThingComp
    {
        private int elapsedTicks;
        private CompProperties_DestroyByTime Props => (CompProperties_DestroyByTime) props;

        private int DestroyTicks => Props.destroyTicks;

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
            if (elapsedTicks < DestroyTicks)
            {
                return;
            }

            var t = parent;

            t.holdingOwner?.Remove(t);

            t.Destroy();
        }
    }
}