using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod;

public class JobDriver_WaterFarm : JobDriver
{
    public const float ConsumeWaterVolume = 1f;

    private const TargetIndex ToolInd = TargetIndex.B;

    private const TargetIndex ToolPlaceInd = TargetIndex.C;

    private const TargetIndex WateringInd = TargetIndex.A;

    private const int WorkingTicks = 60;

    private ThingWithComps Tool => (ThingWithComps)job.GetTarget(ToolInd).Thing;

    private IntVec3 WateringPos => job.GetTarget(WateringInd).Cell;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        pawn.ReserveAsManyAsPossible(job.GetTargetQueue(WateringInd), job);
        pawn.Reserve(Tool, job);
        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        // ツールまで移動
        yield return Toils_Goto.GotoThing(ToolInd, PathEndMode.Touch).FailOnDespawnedNullOrForbidden(ToolInd);

        // ツールを手に取る
        yield return Toils_Haul.StartCarryThing(ToolInd);

        // ターゲットが水やり対象として不適になっていたらリストから外す
        var initExtractTargetFromQueue = Toils_Mizu.ClearConditionSatisfiedTargets(
            WateringInd,
            lti =>
            {
                var mapComp = Map.GetComponent<MapComponent_Watering>();
                return mapComp.Get(Map.cellIndices.CellToIndex(lti.Cell)) > 0;
            });
        yield return initExtractTargetFromQueue;

        yield return Toils_JobTransforms.SucceedOnNoTargetInQueue(WateringInd);

        // ターゲットキューから次のターゲットを取り出す
        yield return Toils_JobTransforms.ExtractNextTargetFromQueue(WateringInd);

        // ターゲットの元へ移動
        yield return Toils_Goto.GotoCell(WateringInd, PathEndMode.Touch);

        // 作業中
        var workToil = new Toil
        {
            initAction = delegate
            {
                // 必要工数の計算
                ticksLeftThisToil = WorkingTicks;
            },

            // 細々とした設定
            defaultCompleteMode = ToilCompleteMode.Delay
        };
        workToil.WithProgressBar(WateringInd, () => 1f - ((float)ticksLeftThisToil / WorkingTicks), true);
        workToil.PlaySustainerOrSound(() => SoundDefOf.Interact_CleanFilth);
        yield return workToil;

        // 作業終了
        var finishToil = new Toil
        {
            initAction = () =>
            {
                // 水やり更新
                var mapComp = Map.GetComponent<MapComponent_Watering>();
                mapComp.Set(
                    Map.cellIndices.CellToIndex(WateringPos),
                    MapComponent_Watering.MaxWateringValue);
                Map.mapDrawer.SectionAt(WateringPos).dirtyFlags = MapMeshFlag.Terrain;

                // ツールから水を減らす
                var compTool = Tool.GetComp<CompWaterTool>();
                compTool.StoredWaterVolume -= ConsumeWaterVolume;
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return finishToil;

        // 最初に戻る
        yield return Toils_Jump.JumpIf(
            initExtractTargetFromQueue,
            () => pawn.jobs.curJob.GetTargetQueue(WateringInd).Count > 0);

        // ツールを片付ける場所を決める
        yield return Toils_Mizu.TryFindStoreCell(ToolInd, ToolPlaceInd);

        // 倉庫まで移動
        yield return Toils_Goto.GotoCell(ToolPlaceInd, PathEndMode.Touch);

        // 倉庫に置く
        yield return Toils_Haul.PlaceHauledThingInCell(ToolPlaceInd, null, true);
    }
}