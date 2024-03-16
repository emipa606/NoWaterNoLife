using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod;

internal class JobDriver_WaterFeedPatient : JobDriver
{
    private const TargetIndex PatientIndex = TargetIndex.B;

    private const TargetIndex WaterIndex = TargetIndex.A;

    private bool getItemFromInventory;

    private Pawn Patient => TargetB.Thing as Pawn;

    private ThingWithComps WaterThing => TargetA.Thing as ThingWithComps;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref getItemFromInventory, "getItemFromInventory");
    }

    public override void Notify_Starting()
    {
        base.Notify_Starting();
        getItemFromInventory = pawn.inventory != null && pawn.inventory.Contains(TargetA.Thing);
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        pawn.Reserve(WaterThing, job);
        pawn.Reserve(Patient, job);
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        // ターゲットがThing=水アイテムを摂取する場合

        // 水(食事)が使用不可能になったらFail
        this.FailOnDestroyedNullOrForbidden(WaterIndex);
        this.FailOn(
            () =>
            {
                if (Patient == null)
                {
                    return true;
                }

                // 患者がベッドに入ってなかったらFail
                if (!Patient.InBed())
                {
                    return true;
                }

                // 到達不能になっていたらFail
                return !pawn.CanReach(Patient, PathEndMode.ClosestTouch, Danger.Deadly);
            });

        // 水が予約出来ない状態なら中断
        if (!pawn.CanReserveAndReach(TargetA, PathEndMode.Touch, Danger.Deadly, 1, job.count))
        {
            GetActor().jobs.EndCurrentJob(JobCondition.Incompletable);
            yield break;
        }

        // 水を予約する
        if (!pawn.Map.reservationManager.ReservedBy(TargetA.Thing, pawn))
        {
            yield return Toils_Reserve.Reserve(WaterIndex, 1, job.count);
        }

        if (getItemFromInventory)
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

        // 患者のもとへ移動
        yield return Toils_Goto.Goto(PatientIndex, PathEndMode.Touch);

        // 水を飲ませる
        yield return Toils_Mizu.FeedToPatient(WaterIndex, PatientIndex);

        // 水の摂取終了(心情、食事の処理)
        yield return Toils_Mizu.FinishDrinkPatient(WaterIndex, PatientIndex);

        if (getItemFromInventory && !TargetA.ThingDestroyed)
        {
            // 所持品から取り出した＆まだ残っている場合は所持品に戻す
            yield return Toils_Mizu.AddCarriedThingToInventory();
        }
    }
}