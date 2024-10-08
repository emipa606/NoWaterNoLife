﻿using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MizuMod;

public class JobGiver_PackWater : ThinkNode_JobGiver
{
    public const WaterPreferability MinWaterPreferability = WaterPreferability.SeaWater;

    private const int ContinuousPackIntervalTick = 150;

    private const float MinWaterPerColonistToDo = 1.5f;

    private const float NeedTotalWaterAmount = 1.0f;

    protected override Job TryGiveJob(Pawn pawn)
    {
        // 所持品インスタンスがない
        if (pawn.inventory == null)
        {
            return null;
        }

        // 水分要求がない
        var need_water = pawn.needs.Water();
        if (need_water == null)
        {
            return null;
        }

        // 既に条件を満たしたアイテムを持っているか？
        foreach (var thing in pawn.inventory.innerContainer)
        {
            if (!validator(thing))
            {
                continue;
            }

            return null;
        }

        // マップ中の水アイテムの合計水分量が、最低限必要とされる水の量(×入植者の人数)以下しかなければ、
        // 個人の所持品には入れない
        if (pawn.Map.resourceCounter.TotalWater() <
            pawn.Map.mapPawns.ColonistsSpawnedCount * MinWaterPerColonistToDo)
        {
            return null;
        }

        var waterThing = GenClosest.ClosestThing_Regionwise_ReachablePrioritized(
            pawn.Position,
            pawn.Map,
            ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
            PathEndMode.ClosestTouch,
            TraverseParms.For(pawn),
            20f,
            t =>
            {
                if (!validator(t))
                {
                    return false; // 所持品チェック時と同じ条件を満たしていない×
                }

                if (t.IsForbidden(pawn))
                {
                    return false; // 禁止されている×
                }

                return pawn.CanReserve(t) &&
                       // 予約不可能×
                       t.IsSociallyProper(pawn);

                // 囚人部屋の物×
            }, // スコアの高いものが優先？
            x => MizuUtility.GetWaterItemScore(pawn, x, 0f, true));

        if (waterThing == null)
        {
            return null;
        }

        return new Job(JobDefOf.TakeInventory, waterThing)
        {
            count = Mathf.Min(MizuUtility.StackCountForWater(waterThing, NeedTotalWaterAmount),
                waterThing.stackCount)
        };

        bool validator(Thing t)
        {
            // 食べられるものは携帯飲料としては選ばれない
            if (t.def.IsIngestible)
            {
                return false;
            }

            var comp = t.TryGetComp<CompWaterSource>();
            if (comp == null)
            {
                return false; // 水源でないもの×
            }

            if (!comp.IsWaterSource)
            {
                return false; // 水源でないもの×
            }

            if (comp.SourceType != CompProperties_WaterSource.SourceType.Item)
            {
                return false; // 水アイテムではないもの×
            }

            if (comp.WaterAmount * comp.MaxNumToGetAtOnce < Need_Water.MinWaterAmountPerOneDrink)
            {
                return false; // 最低水分量を満たしていないもの×
            }

            return MizuDef.Dic_WaterTypeDef[comp.WaterType].waterPreferability >= MinWaterPreferability;
            // 最低の水質を満たしていないもの×
        }
    }
}