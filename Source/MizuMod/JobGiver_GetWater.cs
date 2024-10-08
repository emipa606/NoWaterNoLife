﻿// using System.Text;

using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod;

public class JobGiver_GetWater : ThinkNode_JobGiver
{
    // private const int MaxDistanceOfSearchWaterTerrain = 300;
    private const int SearchWaterIntervalTick = 180;

    private ThirstCategory minCategory = ThirstCategory.SlightlyThirsty;

    public override ThinkNode DeepCopy(bool resolve = true)
    {
        if (base.DeepCopy(resolve) is not JobGiver_GetWater jobGiver_GetWater)
        {
            return null;
        }

        jobGiver_GetWater.minCategory = minCategory;
        return jobGiver_GetWater;
    }

    public override float GetPriority(Pawn pawn)
    {
        var need_water = pawn.needs.Water();

        if (need_water == null)
        {
            return 0.0f;
        }

        // 喉が渇いていないなら0
        if (need_water.CurCategory < minCategory)
        {
            return 0.0f;
        }

        // 何らかの精神崩壊状態の場合、脱水症状が起こるまで水を飲もうとしない
        // →ThinkTreeDefのほうで制御できそう
        if (pawn.MentalState != null && need_water.CurCategory <= ThirstCategory.UrgentlyThirsty)
        {
            return 0.0f;
        }

        // 人間＆(プレイヤー派閥or囚人)でない場合は、マップ内に敵がいるかどうかで条件を変更
        // →ポーンの所持品に水が無い場合は1段階先まで我慢させるか？
        // →段階的に対応してバグ対処したい。これは後回し。
        if (!pawn.RaceProps.Humanlike || pawn.Faction == Faction.OfPlayer || pawn.IsPrisoner)
        {
            return 9.4f;
        }

        foreach (var faction in Find.FactionManager.AllFactionsListForReading)
        {
            var pawnList = pawn.Map.mapPawns.SpawnedPawnsInFaction(faction).Where(p => !p.IsPrisoner);

            // 敵対派閥のポーンがマップ内に居る場合は脱水症状が出るまで我慢
            if (!pawn.HostileTo(faction) || !pawnList.Any())
            {
                continue;
            }

            if (need_water.CurCategory <= ThirstCategory.UrgentlyThirsty)
            {
                return 0.0f;
            }
        }

        return 9.4f;
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        var need_water = pawn.needs.Water();
        if (need_water == null)
        {
            return null;
        }

        // 最後に水を探してから少し経つまで次の探索はしない
        if (need_water.lastSearchWaterTick + SearchWaterIntervalTick > Find.TickManager.TicksGame)
        {
            return null;
        }

        need_water.lastSearchWaterTick = Find.TickManager.TicksGame;
        IntVec3 hiddenWaterSpot;
        // 水の供給源を探す
        var thing = MizuUtility.TryFindBestWaterSourceFor(pawn, pawn, false);
        if (thing == null)
        {
            return MizuUtility.TryFindHiddenWaterSpot(pawn, out hiddenWaterSpot)
                ? new Job(MizuDef.Job_DrinkWater, hiddenWaterSpot) { count = 1 }
                :
                // 水を発見できず
                null;
        }

        if (thing.CanDrinkWater())
        {
            // 水アイテムが見つかった
            return new Job(MizuDef.Job_DrinkWater, thing)
                { count = MizuUtility.WillGetStackCountOf(pawn, thing) };
        }

        if (thing is IBuilding_DrinkWater)
        {
            // 水を汲める設備が見つかった
            return new Job(MizuDef.Job_DrinkWaterFromBuilding, thing);
        }

        // 何も見つからなかった場合は隠し水飲み場を探す
        // 人間、家畜、野生の動物全て
        return MizuUtility.TryFindHiddenWaterSpot(pawn, out hiddenWaterSpot)
            ? new Job(MizuDef.Job_DrinkWater, hiddenWaterSpot) { count = 1 }
            :
            // 水を発見できず
            null;
    }
}