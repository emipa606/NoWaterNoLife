using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_WaterDeliver : JobDriver
    {
        private const TargetIndex DropSpotIndex = TargetIndex.C;

        private const TargetIndex PrisonerIndex = TargetIndex.B;

        private const TargetIndex WaterIndex = TargetIndex.A;

        private bool drinkingFromInventory;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref drinkingFromInventory, "drinkingFromInventory");
        }

        public override void Notify_Starting()
        {
            base.Notify_Starting();
            drinkingFromInventory = pawn.inventory != null && pawn.inventory.Contains(TargetA.Thing);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // 水が使用不可能になったらFail
            this.FailOnDestroyedNullOrForbidden(WaterIndex);

            if (!pawn.CanReserveAndReach(TargetA, PathEndMode.Touch, Danger.Deadly, 1, job.count))
            {
                // 水を予約できなかったら終了
                pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
                yield break;
            }

            // 予約する
            if (!pawn.Map.reservationManager.ReservedBy(TargetA.Thing, pawn))
            {
                yield return Toils_Reserve.Reserve(WaterIndex, 1, job.count);
            }

            if (drinkingFromInventory)
            {
                // 水を持ち物から取り出す
                yield return Toils_Mizu.StartCarryFromInventory(WaterIndex);
            }
            else
            {
                // 水の場所まで行く
                yield return Toils_Goto.Goto(WaterIndex, PathEndMode.OnCell);

                // 水を拾う
                yield return Toils_Ingest.PickupIngestible(WaterIndex, pawn);
            }

            // スポットまで移動する
            yield return Toils_Goto.GotoCell(TargetC.Cell, PathEndMode.OnCell);

            // 置いて囚人に予約させる
            yield return Toils_Mizu.DropCarriedThing(PrisonerIndex, DropSpotIndex);
        }
    }
}