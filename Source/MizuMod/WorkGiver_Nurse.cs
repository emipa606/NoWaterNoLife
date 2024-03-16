using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MizuMod;

public class WorkGiver_Nurse : WorkGiver_TendOther
{
    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        // 対象がポーンではない
        if (t is not Pawn giver)
        {
            return false;
        }

        // 自分自身の看病は出来ない
        if (pawn == t)
        {
            return false;
        }

        // 対象を予約できない
        if (!pawn.CanReserve(giver, 1, -1, null, forced))
        {
            return false;
        }

        // 人間用WorkGiverで相手が人間、または動物用WorkGiverで相手が動物、の組み合わせでない
        if (!(def.feedHumanlikesOnly && giver.RaceProps.Humanlike
              || def.feedAnimalsOnly && giver.RaceProps.Animal))
        {
            return false;
        }

        // 治療可能な体勢になっていない
        if (!GoodLayingStatusForTend(giver, pawn))
        {
            return false;
        }

        // 免疫を得て直すタイプの健康状態を持っていない
        // (治療状態は問わない)
        if (!giver.health.hediffSet.hediffs.Any(hediff => hediff.def.PossibleToDevelopImmunityNaturally()))
        {
            return false;
        }

        // 看病された効果が残っている
        if (giver.health.hediffSet.GetFirstHediffOfDef(MizuDef.Hediff_Nursed) != null)
        {
            return false;
        }

        // 看病アイテムのチェック
        var mopList = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways).Where(
            thing =>
            {
                // 使用禁止チェック
                if (thing.IsForbidden(pawn))
                {
                    return false;
                }

                var comp = thing.TryGetComp<CompWaterTool>();
                if (comp == null)
                {
                    return false;
                }

                if (!comp.UseWorkType.Contains(CompProperties_WaterTool.UseWorkType.Nurse))
                {
                    return false;
                }

                // 1回も使えないレベルの保有水量だったらダメ
                return !(Mathf.Floor(comp.StoredWaterVolume / JobDriver_Nurse.ConsumeWaterVolume / 0.79f) <= 0);
            });
        if (!mopList.Any())
        {
            return false;
        }

        return mopList.Count(thing => pawn.CanReserve(thing)) != 0;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        // 看病ジョブ作成
        var job = new Job(MizuDef.Job_Nurse);
        job.SetTarget(TargetIndex.A, t);

        // 一番近いツールを探す
        Thing candidateTool = null;
        var minDist = int.MaxValue;
        var toolList = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways).Where(
            thing =>
            {
                // 使用禁止チェック
                if (thing.IsForbidden(pawn))
                {
                    return false;
                }

                var comp = thing.TryGetComp<CompWaterTool>();
                if (comp == null)
                {
                    return false;
                }

                if (!comp.UseWorkType.Contains(CompProperties_WaterTool.UseWorkType.Nurse))
                {
                    return false;
                }

                // 1回も使えないレベルの保有水量だったらダメ
                // 80%未満で水を補充するので80%程度であれば使用可能とする
                return !(Mathf.Floor(comp.StoredWaterVolume / JobDriver_Nurse.ConsumeWaterVolume / 0.79f) <= 0);
            });
        foreach (var tool in toolList)
        {
            // 予約できないツールはパス
            if (!pawn.CanReserve(tool))
            {
                continue;
            }

            var toolDist = (tool.Position - pawn.Position).LengthHorizontalSquared;
            if (minDist <= toolDist)
            {
                continue;
            }

            minDist = toolDist;
            candidateTool = tool;
        }

        if (candidateTool == null)
        {
            Log.Error("candidateTool is null");
            return null;
        }

        // ツールをTargetBにセット
        job.SetTarget(TargetIndex.B, candidateTool);
        job.count = 1;

        return job;
    }
}