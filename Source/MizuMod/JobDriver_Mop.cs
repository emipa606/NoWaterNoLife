using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod;

public class JobDriver_Mop : JobDriver
{
    public const float ConsumeWaterVolume = 0.05f;

    private const TargetIndex MopInd = TargetIndex.B;

    private const TargetIndex MoppingInd = TargetIndex.A;

    private const int MoppingTicks = 60;

    private const TargetIndex MopPlaceInd = TargetIndex.C;

    private ThingWithComps Mop => (ThingWithComps)job.GetTarget(MopInd).Thing;

    private IntVec3 MoppingPos => job.GetTarget(MoppingInd).Cell;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        pawn.ReserveAsManyAsPossible(job.GetTargetQueue(MoppingInd), job);
        pawn.Reserve(Mop, job);
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        // モップまで移動
        yield return Toils_Goto.GotoThing(MopInd, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(MopInd);

        // モップを手に取る
        yield return Toils_Haul.StartCarryThing(MopInd);

        // ターゲットが掃除対象として不適になっていたらリストから外す
        // Thing系にしか使えない
        var initExtractTargetFromQueue = Toils_Mizu.ClearConditionSatisfiedTargets(
            MoppingInd,
            lti => lti.Cell.GetFirstThing(pawn.Map, MizuDef.Thing_MoppedThing) != null);
        yield return initExtractTargetFromQueue;

        yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(MoppingInd);

        // ターゲットキューから次のターゲットを取り出す
        yield return Toils_JobTransforms.ExtractNextTargetFromQueue(MoppingInd);

        // ターゲットの元へ移動
        yield return Toils_Goto.GotoCell(MoppingInd, PathEndMode.Touch).JumpIf(
            () =>
            {
                var target = pawn.jobs.curJob.GetTarget(MoppingInd);
                if (target.HasThing)
                {
                    return true;
                }

                return target.Cell.GetFirstThing(pawn.Map, MizuDef.Thing_MoppedThing) != null;
            },
            initExtractTargetFromQueue).JumpIfOutsideMopArea(MoppingInd, initExtractTargetFromQueue);

        // モップ掛け作業中
        var mopToil = new Toil
        {
            initAction = delegate
            {
                // 必要工数の計算
                ticksLeftThisToil = MoppingTicks;
            },

            // 細々とした設定
            defaultCompleteMode = ToilCompleteMode.Delay
        };
        mopToil.WithProgressBar(MoppingInd, () => 1f - ((float)ticksLeftThisToil / MoppingTicks), true);
        mopToil.PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);

        // 掃除中に条件が変更されたら最初に戻る
        mopToil.JumpIf(
            () =>
            {
                var target = pawn.jobs.curJob.GetTarget(MoppingInd);
                if (target.HasThing)
                {
                    return true;
                }

                return target.Cell.GetFirstThing(pawn.Map, MizuDef.Thing_MoppedThing) != null;
            },
            initExtractTargetFromQueue);
        mopToil.JumpIfOutsideMopArea(MoppingInd, initExtractTargetFromQueue);
        yield return mopToil;

        // モップ掛け終了
        var finishToil = new Toil
        {
            initAction = () =>
            {
                // モップオブジェクト生成
                var moppedThing = ThingMaker.MakeThing(MizuDef.Thing_MoppedThing);
                GenSpawn.Spawn(moppedThing, MoppingPos, mopToil.actor.Map);

                // モップから水を減らす
                var compTool = Mop.GetComp<CompWaterTool>();
                compTool.StoredWaterVolume -= ConsumeWaterVolume;
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return finishToil;

        // 最初に戻る
        yield return Toils_Jump.JumpIf(
            initExtractTargetFromQueue,
            () => pawn.jobs.curJob.GetTargetQueue(MoppingInd).Count > 0);

        // モップを片付ける場所を決める
        yield return Toils_Mizu.TryFindStoreCell(MopInd, MopPlaceInd);

        // 倉庫まで移動
        yield return Toils_Goto.GotoCell(MopPlaceInd, PathEndMode.Touch);

        // 倉庫に置く
        yield return Toils_Haul.PlaceHauledThingInCell(MopPlaceInd, null, true);
    }
}