using System.Collections.Generic;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_DrawWaterByPrisoner : JobDriver
    {
        private const TargetIndex DrawerIndex = TargetIndex.A;

        private const int DrawTicks = 500;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var drawer = job.targetA.Thing;
            var peMode = drawer.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.ClosestTouch;

            yield return Toils_Goto.GotoThing(DrawerIndex, peMode);

            yield return Toils_Mizu.DrawWater(DrawerIndex, DrawTicks);

            yield return Toils_Mizu.FinishDrawWater(DrawerIndex);
        }
    }
}