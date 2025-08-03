using System;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MizuMod;

public static class Mizu_Extensions
{
    public static bool CanDrinkFromTerrain(this Pawn pawn)
    {
        // 心情無し = 地面から水をすすることに抵抗なし
        if (pawn.needs?.mood == null)
        {
            return true;
        }

        var need_water = pawn.needs.Water();

        // 水分要求なし = そもそも水を必要としていない
        if (need_water == null)
        {
            return false;
        }

        // 心情有り、水分要求あり、状態が脱水症状 = (心情悪化するけど)地形から水を摂取する
        return need_water.CurCategory == ThirstCategory.Dehydration;
        // 心情あり、水分要求あり、状態はまだ大丈夫 = 地形から水を摂取しない
    }

    public static bool CanDrinkWater(this Thing t)
    {
        var comp = t.TryGetComp<CompWaterSource>();
        return comp is { SourceType: CompProperties_WaterSource.SourceType.Item };
    }

    public static bool CanDrinkWaterNow(this Thing t)
    {
        return !t.IsBurning() && t.CanDrinkWater();
    }

    public static bool CanGetWater(this Thing t)
    {
        if (!t.CanDrinkWater())
        {
            return false;
        }

        var comp = t.TryGetComp<CompWaterSource>();
        return comp.WaterAmount > 0.0f;
    }

    public static bool CanGetWater(this TerrainDef def)
    {
        return def.IsRiver() || def.IsLakeOrPond() || def.IsMarsh() || def.IsSea();
    }

    public static bool CanManipulate(this Pawn pawn)
    {
        return !pawn.IsWildMan() && pawn.RaceProps.ToolUser &&
               pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
    }

    public static Toil ClearCondifionSatisfiedTargets(
        this Toil toil,
        TargetIndex ind,
        Predicate<LocalTargetInfo> cond)
    {
        toil.initAction = delegate
        {
            var actor = toil.actor;
            var curJob = actor.jobs.curJob;
            var targetQueue = curJob.GetTargetQueue(ind);
            targetQueue.RemoveAll(cond);
        };
        return toil;
    }

    public static WaterType GetMinType(this WaterType me, WaterType other)
    {
        return (WaterType)Mathf.Min((int)me, (int)other);
    }

    public static int GetRoofNumNearby(this IBuilding_WaterNet t, int dist)
    {
        var roofNum = 0;
        foreach (var cell in t.OccupiedRect().ExpandedBy(dist))
        {
            if (!cell.InBounds(t.Map))
            {
                continue;
            }

            // 自然の屋根でなければボーナス
            var roofDef = t.Map.roofGrid.RoofAt(cell);
            if (roofDef is { isNatural: false })
            {
                roofNum++;
            }
        }

        return roofNum;
    }

    public static float GetThirstRateFactor(this HediffSet hediffSet)
    {
        var rate = 1f;
        foreach (var hediff in hediffSet.hediffs)
        {
            var hediffStageIndex = hediff.CurStageIndex;
            var ext = hediff.def.GetModExtension<DefExtension_ThirstRate>();
            if (ext?.thirstRateFactors == null)
            {
                continue;
            }

            if (hediffStageIndex < ext.thirstRateFactors.Count)
            {
                rate *= ext.thirstRateFactors[hediffStageIndex];
            }
        }

        foreach (var hediff in hediffSet.hediffs)
        {
            var hediffStageIndex = hediff.CurStageIndex;
            var ext = hediff.def.GetModExtension<DefExtension_ThirstRate>();
            if (ext?.thirstRateFactorOffsets == null)
            {
                continue;
            }

            if (hediffStageIndex < ext.thirstRateFactorOffsets.Count)
            {
                rate += ext.thirstRateFactorOffsets[hediffStageIndex];
            }
        }

        return rate;
    }

    public static float GetUnroofedPercent(this IBuilding_WaterNet t)
    {
        var allCells = 0;
        var unroofedCells = 0;
        foreach (var c in t.OccupiedRect())
        {
            allCells++;
            if (!c.Roofed(t.Map))
            {
                unroofedCells++;
            }
        }

        if (allCells == 0)
        {
            return 0f;
        }

        return (float)unroofedCells / allCells;
    }

    public static float GetWaterAmount(this Thing t)
    {
        var comp = t.TryGetComp<CompWaterSource>();
        if (comp == null)
        {
            return 0f;
        }

        return comp.SourceType != CompProperties_WaterSource.SourceType.Item ? 0f : Math.Max(comp.WaterAmount, 0.0f);
    }

    public static void GetWaterCalculateAmounts(
        this Thing t,
        float waterWanted,
        bool withIngested,
        out int numTaken,
        out float waterGot)
    {
        var waterAmount = t.GetWaterAmount();
        if (waterAmount == 0.0f)
        {
            // 水分量0のアイテムの必要個数を計算させようとしている
            Log.Message("[NoWaterNoLife]: error in GetWaterCalculateAmounts : waterAmount == 0.0f");
            numTaken = 0;
            waterGot = 0.0f;
            return;
        }

        if (!withIngested && t.def.IsIngestible)
        {
            // 食べられるものを飲もうとしている
            Log.Message("[NoWaterNoLife]: error thing is ingestible.");
            numTaken = 0;
            waterGot = 0.0f;
            return;
        }

        numTaken = (int)Math.Ceiling(waterWanted / waterAmount); // そのアイテムで必要な水分を満たすのに何個必要か
        numTaken = Mathf.Min(
            numTaken,
            t.stackCount,
            t.TryGetComp<CompWaterSource>().MaxNumToGetAtOnce); // 必要数、スタック数、同時摂取可能数のうち最も低い数字
        numTaken = Math.Max(numTaken, 1); // 最低値は1
        waterGot = numTaken * waterAmount; // 個数と1個当たりの水分の積→摂取水分量
    }

    public static WaterPreferability GetWaterPreferability(this Thing t)
    {
        var comp = t.TryGetComp<CompWaterSource>();
        if (comp == null)
        {
            return WaterPreferability.Undefined;
        }

        return !comp.IsWaterSource
            ? WaterPreferability.Undefined
            : MizuDef.Dic_WaterTypeDef[comp.WaterType].waterPreferability;
    }

    public static WaterTerrainType GetWaterTerrainType(this TerrainDef def)
    {
        if (def.IsSea())
        {
            return WaterTerrainType.SeaWater;
        }

        if (def.IsRiver())
        {
            return WaterTerrainType.RawWater;
        }

        if (def.IsLakeOrPond())
        {
            return WaterTerrainType.RawWater;
        }

        return def.IsMarsh() ? WaterTerrainType.MudWater : WaterTerrainType.NoWater;
    }

    public static WaterTerrainType GetWaterTerrainType(this Caravan caravan)
    {
        if (caravan.Tile < 0)
        {
            return WaterTerrainType.NoWater;
        }

        var tile = Find.WorldGrid[caravan.Tile];

        var result = WaterTerrainType.NoWater;

        // バイオーム
        var biomeDefName = tile.PrimaryBiome.defName;
        if (biomeDefName.Contains("Desert"))
        {
            // 砂漠Desert、極限の砂漠ExtremeDesertは水なし
            result = (WaterTerrainType)Mathf.Max((int)result, (int)WaterTerrainType.NoWater);
        }
        else if (biomeDefName.Contains("SeaIce"))
        {
            // 海氷SeaIceは海水
            result = (WaterTerrainType)Mathf.Max((int)result, (int)WaterTerrainType.SeaWater);
        }
        else if (biomeDefName.Contains("ColdBog") || biomeDefName.Contains("Swamp"))
        {
            // 寒冷湿地ColdBog、温帯湿地TemperateSwamp、熱帯湿地TropicalSwampは泥水
            result = (WaterTerrainType)Mathf.Max((int)result, (int)WaterTerrainType.MudWater);
        }
        else
        {
            // それ以外は真水
            result = (WaterTerrainType)Mathf.Max((int)result, (int)WaterTerrainType.RawWater);
        }

        if (((SurfaceTile)tile).Rivers is { Count: > 0 })
        {
            // 川があれば真水が飲める(凍ってるか等はチェックしないことにする)
            result = (WaterTerrainType)Mathf.Max((int)result, (int)WaterTerrainType.RawWater);
        }

        if (Find.World.CoastDirectionAt(caravan.Tile).IsValid)
        {
            // 海岸線があるなら海水が飲める
            result = (WaterTerrainType)Mathf.Max((int)result, (int)WaterTerrainType.SeaWater);
        }

        return result;
    }

    public static bool IsConnectedAnd(
        this IBuilding_WaterNet t1,
        IBuilding_WaterNet t2,
        bool ignoreActivate = false)
    {
        return t1.IsOutputTo(t2, ignoreActivate) && t2.IsOutputTo(t1, ignoreActivate);
    }

    public static bool IsConnectedOr(this IBuilding_WaterNet t1, IBuilding_WaterNet t2, bool ignoreActivate = false)
    {
        return t1.IsOutputTo(t2, ignoreActivate) || t2.IsOutputTo(t1, ignoreActivate);
    }

    public static bool IsIce(this TerrainDef def)
    {
        return def.defName == "Ice";
    }

    public static bool IsIngestibleFor(this Thing thing, Pawn pawn)
    {
        return thing.def.IsNutritionGivingIngestible && thing.IngestibleNow && pawn.RaceProps.CanEverEat(thing);
    }

    public static bool IsLakeOrPond(this TerrainDef def)
    {
        return !def.IsSea() && !def.IsRiver() && def.defName.Contains("Water");
    }

    public static bool IsMarsh(this TerrainDef def)
    {
        return def.defName.Contains("Marsh");
    }

    public static bool IsOutputTo(this IBuilding_WaterNet t1, IBuilding_WaterNet t2, bool ignoreActivate = false)
    {
        switch (ignoreActivate)
        {
            case false when !t1.HasOutputConnector:
            case false when !t2.HasInputConnector:
                return false;
        }

        var t1_out_t2_body = false;
        foreach (var vec1 in t1.OutputConnectors)
        {
            foreach (var vec2 in t2.OccupiedRect())
            {
                if (vec1 != vec2)
                {
                    continue;
                }

                t1_out_t2_body = true;
                break;
            }

            if (t1_out_t2_body)
            {
                break;
            }
        }

        var t1_body_t2_in = false;
        foreach (var vec1 in t1.OccupiedRect())
        {
            foreach (var vec2 in t2.InputConnectors)
            {
                if (vec1 != vec2)
                {
                    continue;
                }

                t1_body_t2_in = true;
                break;
            }

            if (t1_body_t2_in)
            {
                break;
            }
        }

        return t1_out_t2_body && t1_body_t2_in;
    }

    public static bool IsRiver(this TerrainDef def)
    {
        return def.defName.Contains("WaterMoving");
    }

    public static bool IsRotSoonForWater(this Thing t)
    {
        // 水でなければfalse
        if (!t.CanGetWater())
        {
            return false;
        }

        var compRottable = t.TryGetComp<CompRottable>();
        if (compRottable == null)
        {
            return false;
        }

        if (compRottable.Stage != RotStage.Fresh)
        {
            return false;
        }

        return compRottable.TicksUntilRotAtCurrentTemp < 60000;
        // 腐る水で、現在は新鮮で、あと1日以内に腐ってしまう場合、腐りかけと判断する
    }

    public static bool IsSea(this TerrainDef def)
    {
        return def.defName.Contains("WaterOcean");
    }

    public static bool IsWater(this TerrainDef def)
    {
        return def.IsSea() || def.IsRiver() || def.IsLakeOrPond() || def.IsMarsh();
    }

    public static bool IsWaterStandable(this TerrainDef def)
    {
        return def.IsWater() && def.passability == Traversability.Standable;
    }

    public static Toil JumpIfOutsideMopArea(this Toil toil, TargetIndex ind, Toil jumpToil)
    {
        return toil.JumpIf(
            delegate
            {
                var target = toil.actor.jobs.curJob.GetTarget(ind);
                var pos = target.HasThing ? target.Thing.Position : target.Cell;

                return !toil.actor.Map.areaManager.Mop()[pos];
            },
            jumpToil);
    }

    public static Area_Mop Mop(this AreaManager areaManager)
    {
        var area = areaManager.Get<Area_Mop>();
        if (area != null)
        {
            return area;
        }

        var newArea = new Area_Mop(areaManager);
        areaManager.AllAreas.Add(newArea);
        return newArea;
    }

    public static Area_SnowGet SnowGet(this AreaManager areaManager)
    {
        var area = areaManager.Get<Area_SnowGet>();
        if (area != null)
        {
            return area;
        }

        var newArea = new Area_SnowGet(areaManager);
        areaManager.AllAreas.Add(newArea);
        return newArea;
    }

    public static float TotalWater(this ResourceCounter rc)
    {
        var num = 0.0f;
        foreach (var current in rc.AllCountedAmounts)
        {
            var compprop =
                (CompProperties_WaterSource)current.Key.comps?.Find(c => c.compClass == typeof(CompWaterSource));
            if (compprop == null)
            {
                continue;
            }

            if (compprop.sourceType != CompProperties_WaterSource.SourceType.Item)
            {
                continue;
            }

            if (compprop.waterAmount > 0.0f)
            {
                num += compprop.waterAmount * current.Value;
            }
        }

        return num;
    }

    public static WaterPreferability ToWaterPreferability(this WaterType waterType)
    {
        switch (waterType)
        {
            case WaterType.ClearWater:
                return WaterPreferability.ClearWater;
            case WaterType.NormalWater:
                return WaterPreferability.NormalWater;
            case WaterType.RawWater:
                return WaterPreferability.RawWater;
            case WaterType.MudWater:
                return WaterPreferability.MudWater;
            case WaterType.SeaWater:
                return WaterPreferability.SeaWater;
            default:
                return WaterPreferability.Undefined;
        }
    }

    public static WaterType ToWaterType(this TerrainDef def)
    {
        if (def.IsSea())
        {
            return WaterType.SeaWater;
        }

        if (def.IsMarsh() || def.IsMud())
        {
            return WaterType.MudWater;
        }

        if (def.IsRiver() || def.IsLakeOrPond())
        {
            return WaterType.RawWater;
        }

        return WaterType.NoWater;
    }

    public static Need_Water Water(this Pawn_NeedsTracker needs)
    {
        return needs.TryGetNeed<Need_Water>();
    }

    private static bool IsMud(this TerrainDef def)
    {
        return def.defName.Contains("Mud");
    }
}