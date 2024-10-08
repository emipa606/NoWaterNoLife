﻿using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MizuMod;

public class JobDriver_PourWater : JobDriver_DoBill
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        if (!pawn.Reserve(job.GetTarget(BillGiverInd), job))
        {
            return false;
        }

        var ingList = job.GetTargetQueue(IngredientInd);
        var ingCountList = job.countQueue;
        if (ingList.Count != ingCountList.Count)
        {
            return false;
        }

        for (var i = 0; i < ingList.Count; i++)
        {
            if (!pawn.Reserve(ingList[i], job, 1, ingCountList[i]))
            {
                return false;
            }
        }

        return true;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        var ingList = job.GetTargetQueue(IngredientInd);
        var ingCountList = job.countQueue;

        // this.job.SetTarget(IngredientPlaceCellInd, this.TargetA.Thing.InteractionCell);
        var startToil = Toils_General.Do(
            () =>
            {
                job.SetTarget(IngredientInd, ingList[0].Thing);
                job.count = ingCountList[0];
                ingList.RemoveAt(0);
                ingCountList.RemoveAt(0);
            });

        // 材料キューの先頭を取り出してセット
        yield return startToil;

        // 材料の置き場所へ移動
        var gotoToil = Toils_Goto.GotoThing(IngredientInd, PathEndMode.Touch);
        yield return gotoToil;

        // 材料を運ぶ
        yield return Toils_Haul.StartCarryThing(IngredientInd);

        // 運ぶものリストの中に同種の材料があり、まだ物を持てる場合、設備へ持っていく前に取りに行く
        yield return Toils_General.Do(
            () =>
            {
                var actor = pawn;
                var curJob = actor.jobs.curJob;
                var targetQueue = curJob.GetTargetQueue(IngredientInd);
                if (targetQueue.NullOrEmpty())
                {
                    return;
                }

                if (curJob.count <= 0)
                {
                    return;
                }

                if (actor.carryTracker.CarriedThing == null)
                {
                    Log.Error(
                        $"JumpToAlsoCollectTargetInQueue run on {actor} who is not carrying something.");
                    return;
                }

                if (actor.carryTracker.AvailableStackSpace(actor.carryTracker.CarriedThing.def) <= 0)
                {
                    return;
                }

                for (var i = 0; i < targetQueue.Count; i++)
                {
                    if (!GenAI.CanUseItemForWork(actor, targetQueue[i].Thing))
                    {
                        actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                        return;
                    }

                    if (targetQueue[i].Thing.def != actor.carryTracker.CarriedThing.def)
                    {
                        continue;
                    }

                    curJob.SetTarget(IngredientInd, targetQueue[i].Thing);
                    curJob.count = curJob.countQueue[i];
                    targetQueue.RemoveAt(i);
                    curJob.countQueue.RemoveAt(i);
                    actor.jobs.curDriver.JumpToToil(gotoToil);
                    break;
                }
            });

        // 運ぶ
        yield return Toils_Haul.CarryHauledThingToCell(IngredientPlaceCellInd);

        // 運んだものリスト(使用素材)に追加
        yield return Toils_Mizu.AddPlacedThing();

        // 運んだものを置く
        yield return Toils_Haul.PlaceCarriedThingInCellFacing(BillGiverInd);

        // まだ材料があるならさらに運ぶ
        yield return Toils_General.Do(
            () =>
            {
                if (job.GetTargetQueue(IngredientInd).Count > 0)
                {
                    pawn.jobs.curDriver.JumpToToil(startToil);
                }
            });

        // レシピ実行
        yield return Toils_Recipe.DoRecipeWork();

        // 水の注入完了処理
        yield return Toils_Mizu.FinishPourRecipe(BillGiverInd);
    }
}