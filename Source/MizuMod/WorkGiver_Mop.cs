using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MizuMod;

public class WorkGiver_Mop : WorkGiver_Scanner
{
    public override int MaxRegionsToScanBeforeGlobalSearch => 4;

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
    {
        // プレイヤー派閥でないなら絶対モップ掛けしない
        if (pawn.Faction != Faction.OfPlayer)
        {
            return false;
        }

        // モップエリア外はやらない
        if (pawn.Map.areaManager.Mop()[c] == false)
        {
            return false;
        }

        // 人工フロアかつカーペット以外の場所ならOK
        // 自然地形の汚れが付く＝人工フロア
        // カーペットの研究が必要なもの＝カーペット
        var terrain = c.GetTerrain(pawn.Map);
        if (terrain.filthAcceptanceMask == FilthSourceFlags.None || terrain.researchPrerequisites != null
            && terrain.researchPrerequisites.Contains(ResearchProjectDefOf.CarpetMaking))
        {
            return false;
        }

        // その場所に汚れがあったらやらない
        var thingList = c.GetThingList(pawn.Map);
        var filthList = thingList.Where(t => t is Filth);
        if (filthList.Any())
        {
            return false;
        }

        // 既にモップ掛けされている場所にはやらない
        var moppedThingList = thingList.Where(t => t.def == MizuDef.Thing_MoppedThing);
        if (moppedThingList.Any())
        {
            return false;
        }

        // その場所を予約できないならやらない
        if (!pawn.CanReserve(c))
        {
            return false;
        }

        // モップアイテムのチェック
        var mopList = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways).Where(
            t =>
            {
                // 使用禁止チェック
                if (t.IsForbidden(pawn))
                {
                    return false;
                }

                var comp = t.TryGetComp<CompWaterTool>();
                if (comp == null)
                {
                    return false;
                }

                if (!comp.UseWorkType.Contains(CompProperties_WaterTool.UseWorkType.Mop))
                {
                    return false;
                }

                var maxQueueLength =
                    (int)Mathf.Floor(comp.StoredWaterVolume / JobDriver_Mop.ConsumeWaterVolume);
                if (maxQueueLength <= 0)
                {
                    return false;
                }

                return true;
            });
        if (!mopList.Any())
        {
            return false;
        }

        return mopList.Count(t => pawn.CanReserve(t)) != 0;
    }

    public override Job JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
    {
        // モップジョブ作成
        var job = new Job(MizuDef.Job_Mop);
        job.AddQueuedTarget(TargetIndex.A, cell);

        // 一番近いモップを探す
        Thing candidateMop = null;
        var minDist = int.MaxValue;
        var mopList = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways).Where(
            t =>
            {
                // 使用禁止チェック
                if (t.IsForbidden(pawn))
                {
                    return false;
                }

                var comp = t.TryGetComp<CompWaterTool>();
                if (comp == null)
                {
                    return false;
                }

                if (!comp.UseWorkType.Contains(CompProperties_WaterTool.UseWorkType.Mop))
                {
                    return false;
                }

                var maxQueueLengthForCheck =
                    (int)Mathf.Floor(comp.StoredWaterVolume / JobDriver_Mop.ConsumeWaterVolume);
                return maxQueueLengthForCheck > 0;
            });

        foreach (var mop in mopList)
        {
            // 予約できないモップはパス
            if (!pawn.CanReserve(mop))
            {
                continue;
            }

            var mopDist = (mop.Position - pawn.Position).LengthHorizontalSquared;
            if (minDist <= mopDist)
            {
                continue;
            }

            minDist = mopDist;
            candidateMop = mop;
        }

        if (candidateMop == null)
        {
            Log.Error("candidateMop is null");
            return null;
        }

        // モップをTargetBにセット
        job.targetB = candidateMop;
        job.count = 1;

        var compTool = candidateMop.TryGetComp<CompWaterTool>();
        var maxQueueLength = Mathf.RoundToInt(compTool.StoredWaterVolume / JobDriver_Mop.ConsumeWaterVolume);
        var map = pawn.Map;
        var room = cell.GetRoom(map);
        for (var i = 0; i < 100; i++)
        {
            // 対象の汚れの周囲100マスをサーチ
            var intVec = cell + GenRadial.RadialPattern[i];
            if (!intVec.InBounds(map) || intVec.GetRoom(map) != room)
            {
                continue;
            }

            // そこが同じ部屋の中
            if (HasJobOnCell(pawn, intVec) && intVec != cell)
            {
                // 同じジョブが作成可能(汚れがある等)あるならこのジョブの処理対象に追加
                job.AddQueuedTarget(TargetIndex.A, intVec);
            }

            // 掃除最大個数チェック
            if (job.GetTargetQueue(TargetIndex.A).Count >= maxQueueLength)
            {
                break;
            }
        }

        if (job.targetQueueA is { Count: >= 5 })
        {
            // 掃除対象が5個以上あるならポーンからの距離が近い順に掃除させる
            job.targetQueueA.SortBy(targ => targ.Cell.DistanceToSquared(pawn.Position));
        }

        return job;
    }

    public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
    {
        return pawn.Map.areaManager.Mop().ActiveCells;
    }

    // public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    // {
    // // 掃除ジョブ作成
    // Job job = new Job(MizuDef.Job_Mop);
    // job.AddQueuedTarget(TargetIndex.A, t);

    // int num = 15;
    // Map map = t.Map;
    // Room room = t.GetRoom(RegionType.Set_Passable);
    // for (int i = 0; i < 100; i++)
    // {
    // // 対象の汚れの周囲100マスをサーチ
    // IntVec3 intVec = t.Position + GenRadial.RadialPattern[i];
    // if (intVec.InBounds(map) && intVec.GetRoom(map, RegionType.Set_Passable) == room)
    // {
    // // そこが同じ部屋の中
    // List<Thing> thingList = intVec.GetThingList(map);
    // for (int j = 0; j < thingList.Count; j++)
    // {
    // // その場所にあるThingをチェック
    // Thing thing = thingList[j];
    // if (this.HasJobOnThing(pawn, thing, forced) && thing != t)
    // {
    // // 同じジョブが作成可能(汚れがある等)あるならこののジョブの処理対象に追加
    // job.AddQueuedTarget(TargetIndex.A, thing);
    // }
    // }

    // // 掃除最大個数チェック(15個)
    // if (job.GetTargetQueue(TargetIndex.A).Count >= num)
    // {
    // break;
    // }
    // }
    // }
    // if (job.targetQueueA != null && job.targetQueueA.Count >= 5)
    // {
    // // 掃除対象が5個以上あるならポーンからの距離が近い順に掃除させる
    // job.targetQueueA.SortBy((LocalTargetInfo targ) => targ.Cell.DistanceToSquared(pawn.Position));
    // }

    // return job;
    // }
}