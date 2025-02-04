using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

// using System.Linq;
// using System.Text;
namespace MizuMod;

public static class MizuUtility
{
    private const float SearchWaterRadiusForWildAnimal = 30f;

    private static readonly List<ThoughtDef> thoughtList = [];

    public static void GenerateUndergroundWaterGrid(Map map, MapComponent_WaterGrid waterGrid, int basePoolNum = 30,
        int minWaterPoolNum = 3, float baseRainFall = 1000f, float basePlantDensity = 0.25f,
        float literPerCell = 10.0f, IntRange poolCellRange = default, FloatRange baseRegenRateRange = default,
        float rainRegenRatePerCell = 5.0f)
    {
        if (poolCellRange == default)
        {
            poolCellRange = new IntRange(30, 100);
        }

        if (baseRegenRateRange == default)
        {
            baseRegenRateRange = new FloatRange(10.0f, 20.0f);
        }

        var BaseMapArea = 250f * 250f;

        var rainRate = map.TileInfo.rainfall / baseRainFall;
        var areaRate = map.Area / BaseMapArea;
        var plantRate = map.Biome.plantDensity / basePlantDensity;

        var waterPoolNum = Mathf.RoundToInt(basePoolNum * rainRate * areaRate * plantRate);

        // Log.Message(string.Format("rain={0},area={1},plant={2},num={3}", rainRate.ToString("F3"), areaRate.ToString("F3"), plantRate.ToString("F3"), waterPoolNum));
        if (plantRate > 0.0f)
        {
            waterPoolNum = Mathf.Max(waterPoolNum, minWaterPoolNum);
        }

        for (var i = 0; i < waterPoolNum; i++)
        {
            if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(5,
                    c => !waterGrid.GetCellBool(map.cellIndices.CellToIndex(c)), map, out var result))
            {
                continue;
            }

            var numCells = poolCellRange.RandomInRange;
            var baseRegenRate = baseRegenRateRange.RandomInRange;
            var pool = new UndergroundWaterPool(waterGrid, numCells * literPerCell, WaterType.RawWater,
                baseRegenRate, rainRegenRatePerCell) { ID = i + 1 };
            waterGrid.AddWaterPool(pool, GridShapeMaker.IrregularLump(result, map, numCells));
        }

        waterGrid.ModifyPoolGrid();
    }

    public static ThoughtDef GetThoughtDefFromTerrainType(WaterTerrainType waterTerrainType)
    {
        // 地形タイプ→心情
        switch (waterTerrainType)
        {
            case WaterTerrainType.MudWater:
                return MizuDef.Thought_DrankMudWater;
            case WaterTerrainType.SeaWater:
                return MizuDef.Thought_DrankSeaWater;
            default:
                return null;
        }
    }

    public static float GetWater(Pawn getter, Thing thing, float waterWanted, bool withIngested)
    {
        // 摂取しようとしているものが既に消滅している(エラー)
        // 食事と同時に水分摂取する場合は既に消滅しているので無視する
        if (!withIngested && thing.Destroyed)
        {
            Log.Error($"{getter} drank destroyed thing {thing}");
            return 0f;
        }

        // 現在飲めないはずのものを飲もうとしている(エラー)
        if (!thing.CanDrinkWaterNow())
        {
            Log.Error($"{getter} drank CanDrinkWaterNow()=false thing {thing}");
            return 0f;
        }

        if (getter.needs.mood != null)
        {
            // 水分摂取による心情変化
            foreach (var thoughtDef in ThoughtsFromGettingWater(getter, thing))
            {
                getter.needs.mood.thoughts.memories.TryGainMemory(thoughtDef);
            }
        }

        // 健康状態の変化
        var comp = thing.TryGetComp<CompWaterSource>();
        if (comp == null)
        {
            Log.Error("comp is null");
            return 0.0f;
        }

        if (!comp.IsWaterSource && comp.DependIngredients == false)
        {
            Log.Error("not watersource");
            return 0.0f;
        }

        if (comp.SourceType != CompProperties_WaterSource.SourceType.Item)
        {
            Log.Error("source type is not item");
            return 0.0f;
        }

        var waterType = GetWaterType(thing);
        var waterTypeDef = MizuDef.Dic_WaterTypeDef[waterType];

        // 指定された健康状態になる
        if (waterTypeDef.hediffs != null)
        {
            foreach (var hediff in waterTypeDef.hediffs)
            {
                getter.health.AddHediff(HediffMaker.MakeHediff(hediff, getter));
            }
        }

        // 確率で食中毒
        var animalFactor = !getter.AnimalOrWildMan() ? 1f : 0.1f; // 動物は1/10に抑える
        if (Rand.Value < waterTypeDef.foodPoisonChance * animalFactor)
        {
            FoodUtility.AddFoodPoisoningHediff(getter, thing, FoodPoisonCause.Unknown);
        }

        // 摂取個数と摂取水分量の計算
        thing.GetWaterCalculateAmounts(waterWanted, withIngested, out var drankWaterItemCount,
            out var gotWaterAmount);

        if (withIngested)
        {
            // 食事の場合は後で個数を計算するのでここでは1個にする
            gotWaterAmount = comp.WaterAmount;
            drankWaterItemCount = 1;
        }

        // 食事と同時に水分摂取する場合は既に消滅しているので消滅処理をスキップする
        if (withIngested || drankWaterItemCount <= 0)
        {
            return gotWaterAmount;
        }

        if (drankWaterItemCount == thing.stackCount)
        {
            // アイテム消費数とスタック数が同じ
            // →完全消滅
            thing.Destroy();
        }
        else
        {
            // スタック数と異なる
            // →消費した数だけ減らす
            thing.SplitOff(drankWaterItemCount);
        }

        return gotWaterAmount;
    }

    public static float GetWaterItemScore(Pawn eater, Thing t, float dist, bool priorQuality)
    {
        var comp = t.TryGetComp<CompWaterSource>();

        // 水源ではない or 水源として使えない
        if (comp is not { IsWaterSource: true })
        {
            return float.MinValue;
        }

        // 食べられるものは飲み物としては選ばない方針
        if (t.def.IsIngestible)
        {
            return float.MinValue;
        }

        // 水アイテムなのに水分量が少ない(食事におまけで付いてる水分など)
        // 1個あたりが少なくても、一度に摂取できる量が多い場合は水分摂取アイテムとして有効
        if (comp.SourceType == CompProperties_WaterSource.SourceType.Item &&
            comp.WaterAmount * comp.MaxNumToGetAtOnce < Need_Water.MinWaterAmountPerOneDrink)
        {
            return float.MinValue;
        }

        var waterTypeDef = MizuDef.Dic_WaterTypeDef[comp.WaterType];

        // 基本点計算

        // 距離
        var distScore = -dist;

        // 心情変化量(水質)
        // メモ
        // きれい= +10
        // 普通  =   0
        // 生水  =   0
        // 泥水  =  -6
        // 海水  =  -6
        var thoughtScore = 0f;

        // 禁欲の影響も含まれている
        foreach (var thought in ThoughtsFromGettingWater(eater, t))
        {
            thoughtScore += thought.stages[0].baseMoodEffect;
        }

        // 食中毒
        // メモ
        // きれい= 0    =>   0
        // 普通  = 0    =>   0
        // 生水  = 0.01 => -10
        // 泥水  = 0.03 => -30
        // 海水  = 0.03 => -30
        var foodPoisoningScore = -(waterTypeDef.foodPoisonChance * 1000f);

        // 健康悪化
        if (waterTypeDef.hediffs != null)
        {
        }

        // 腐敗進行度
        var rotScore = 0f;
        if (t.IsRotSoonForWater())
        {
            rotScore += 10f;
        }

        // 飲むのにかかる時間
        var drinkTickScore = 0f;
        switch (comp.SourceType)
        {
            case CompProperties_WaterSource.SourceType.Item:
                drinkTickScore = -comp.BaseDrinkTicks / 100f / comp.WaterAmount;
                break;
            case CompProperties_WaterSource.SourceType.Building:
                drinkTickScore = -comp.BaseDrinkTicks / 100f / 0.35f;
                break;
        }

        // 基本点合計メモ
        // 心情,食中毒,健康,合計(禁欲)
        // きれい= +10,     0,   0, +10(   0)
        // 普通  =   0,     0,   0,   0(   0)
        // 生水  =   0,   -10,   0, -10( -10)
        // 泥水  =  -6,   -30,   0, -36( -30)
        // 海水  =  -6,   -30,-100,-136(-130)

        // 各種状態によるスコアの変化

        // 水質優先モードか否か
        if (priorQuality)
        {
            distScore /= 10f;
        }

        return distScore + thoughtScore + foodPoisoningScore + rotScore + drinkTickScore;
    }

    public static ThingDef GetWaterThingDefFromTerrainType(WaterTerrainType waterTerrainType)
    {
        // 地形タイプ→水アイテム
        switch (waterTerrainType)
        {
            case WaterTerrainType.RawWater:
                return MizuDef.Thing_RawWater;
            case WaterTerrainType.MudWater:
                return MizuDef.Thing_MudWater;
            case WaterTerrainType.SeaWater:
                return MizuDef.Thing_SeaWater;
            default:
                return null;
        }
    }

    public static ThingDef GetWaterThingDefFromWaterPreferability(WaterPreferability waterPreferability)
    {
        // 水の種類→水アイテム
        switch (waterPreferability)
        {
            case WaterPreferability.ClearWater:
                return MizuDef.Thing_ClearWater;
            case WaterPreferability.NormalWater:
                return MizuDef.Thing_NormalWater;
            case WaterPreferability.RawWater:
                return MizuDef.Thing_RawWater;
            case WaterPreferability.MudWater:
                return MizuDef.Thing_MudWater;
            case WaterPreferability.SeaWater:
                return MizuDef.Thing_SeaWater;
            default:
                return null;
        }
    }

    public static ThingDef GetWaterThingDefFromWaterType(WaterType waterType)
    {
        // 水の種類→水アイテム
        switch (waterType)
        {
            case WaterType.ClearWater:
                return MizuDef.Thing_ClearWater;
            case WaterType.NormalWater:
                return MizuDef.Thing_NormalWater;
            case WaterType.RawWater:
                return MizuDef.Thing_RawWater;
            case WaterType.MudWater:
                return MizuDef.Thing_MudWater;
            case WaterType.SeaWater:
                return MizuDef.Thing_SeaWater;
            default:
                return null;
        }
    }

    public static void PrePostIngested(Pawn ingester, Thing t, int num)
    {
        var need_water = ingester.needs.Water();
        if (need_water == null)
        {
            return;
        }

        var comp = t.TryGetComp<CompWaterSource>();
        if (comp == null)
        {
            return;
        }

        num = Math.Max(num, 1);

        // 食事のついでの水分摂取の場合、帰ってくる水分量は常に1個分
        var gotWaterAmount = GetWater(ingester, t, need_water.WaterWanted, true);

        // 後で個数を掛け算する
        gotWaterAmount *= num;

        if (!ingester.Dead)
        {
            need_water.CurLevel += gotWaterAmount;
        }

        ingester.records.AddTo(MizuDef.Record_WaterDrank, gotWaterAmount);
    }

    public static int StackCountForWater(Thing thing, float waterWanted)
    {
        var comp = thing.TryGetComp<CompWaterSource>();

        // 水源ではない
        if (comp is not { IsWaterSource: true })
        {
            return 0;
        }

        // 必要な水分がほぼゼロ
        return waterWanted <= 0.0001f
            ? 0
            :
            // それを何個摂取すれば水分が十分になるのかを返す(最低値1)
            Math.Max((int)Math.Round(waterWanted / comp.WaterAmount), 1);
    }

    public static List<ThoughtDef> ThoughtsFromGettingWater(Pawn getter, Thing t)
    {
        // 空のリスト
        thoughtList.Clear();

        // 心情ステータスの無いポーンには空のリストを返す
        if (getter.needs?.mood == null)
        {
            return thoughtList;
        }

        var comp = t.TryGetComp<CompWaterSource>();
        if (comp == null)
        {
            return thoughtList;
        }

        if (!comp.IsWaterSource)
        {
            return thoughtList;
        }

        var waterType = GetWaterType(t);
        var waterTypeDef = MizuDef.Dic_WaterTypeDef[waterType];

        var isDirect = comp.SourceType == CompProperties_WaterSource.SourceType.Building;
        ThoughtsFromWaterTypeDef(getter, waterTypeDef, isDirect, thoughtList);

        return thoughtList;
    }

    public static void ThoughtsFromWaterTypeDef(Pawn getter, WaterTypeDef waterTypeDef, bool isDirect,
        List<ThoughtDef> thoughtDefs)
    {
        // 禁欲主義は心情の変化を無視する
        if (getter.story?.traits != null && getter.story.traits.HasTrait(TraitDefOf.Ascetic))
        {
            return;
        }

        // 水ごとの飲んだ時の心情が設定されていたら、それを与える
        if (waterTypeDef.thoughts != null)
        {
            thoughtDefs.AddRange(waterTypeDef.thoughts);
        }

        // 飲み方による心情の変化
        if (!isDirect)
        {
            return;
        }

        thoughtDefs.Add(getter.CanManipulate()
            ? MizuDef.Thought_DrankScoopedWater
            : MizuDef.Thought_SippedWaterLikeBeast);
    }

    public static Thing TryFindBestWaterSourceFor(Pawn getter, Pawn eater, bool priorQuality,
        bool canUseInventory = true, bool allowForbidden = false, bool allowSociallyImproper = false)
    {
        // ドラッグ嫌いではない
        // →ドラッグを許可
        var allowDrug = !eater.IsTeetotaler();

        Thing inventoryThing = null;
        if (canUseInventory && getter.CanManipulate())
        {
            // 所持品から探すフラグON、取得者は操作が可能
            // →所持品からベストな飲み物を探す
            inventoryThing = BestWaterInInventory(getter, WaterPreferability.SeaWater,
                WaterPreferability.ClearWater, 0f, allowDrug);
        }

        if (inventoryThing != null)
        {
            // 所持品から見つかり、取得者はプレイヤーではない
            // →そのまま飲む
            if (getter.Faction != Faction.OfPlayer)
            {
                return inventoryThing;
            }

            // プレイヤーだった場合
            // →腐りかけならそのまま飲む
            if (inventoryThing.IsRotSoonForWater())
            {
                return inventoryThing;
            }
        }

        // プレイヤー＆所持品の水は新鮮
        // →マップからも探す
        var mapThing = BestWaterSourceOnMap(getter, eater, priorQuality, WaterPreferability.ClearWater, allowDrug,
            allowForbidden, allowSociallyImproper);

        if (eater.AnimalOrWildMan() && eater.Faction != Faction.OfPlayer)
        {
            // 野生の動物の場合、探したものが一定の距離以上であれば選択肢から除外
            // １個しかない水飲み場に全動物が集まるのを防ぐ
            if (mapThing != null && (eater.Position - mapThing.Position).LengthManhattan >=
                SearchWaterRadiusForWildAnimal)
            {
                mapThing = null;
            }
        }

        switch (inventoryThing)
        {
            case null when mapThing == null:
            {
                // 所持品にまともな水なし、マップからいかなる水も見つけられない
                // →ランクを落として所持品から探しなおす
                if (canUseInventory && getter.CanManipulate())
                {
                    // 見つかっても見つからなくてもその結果を返す
                    return BestWaterInInventory(getter, WaterPreferability.SeaWater, WaterPreferability.ClearWater, 0f,
                        allowDrug);
                }

                // 所持品から探せる状態ではない
                return null;
            }
            // 所持品にまともな水なし、マップから水が見つかった
            // →マップの水を取得
            case null:
                return mapThing;
        }

        // 所持品からまともな水が見つかった、マップからはいかなる水も見つけられない
        // →所持品の水を取得
        if (mapThing == null)
        {
            return inventoryThing;
        }

        // 所持品からまともな水が、マップからは何らかの水が見つかった
        // →どちらが良いか評価(スコアが高い方が良い)
        var scoreMapThing = GetWaterItemScore(eater, mapThing,
            (getter.Position - mapThing.Position).LengthManhattan, priorQuality);
        var scoreInventoryThing = GetWaterItemScore(eater, inventoryThing, 0f, priorQuality);

        // 所持品のほうを優先しやすくする
        scoreInventoryThing += 30f;

        // マップの水のほうが高スコア
        return scoreMapThing > scoreInventoryThing
            ? mapThing
            :
            // 所持品の水のほうが高スコア
            inventoryThing;
    }

    public static bool TryFindHiddenWaterSpot(Pawn pawn, out IntVec3 result)
    {
        var hiddenWaterSpot = pawn.Map.GetComponent<MapComponent_HiddenWaterSpot>();
        if (hiddenWaterSpot == null)
        {
            Log.Error("hiddenWaterSpot is null");
            result = IntVec3.Invalid;
            return false;
        }

        var isFound = false;
        var maxScore = float.MinValue;
        result = IntVec3.Invalid;
        foreach (var c in hiddenWaterSpot.SpotCells)
        {
            float curDist = (pawn.Position - c).LengthManhattan;
            if (!pawn.CanReach(c, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                continue;
            }

            var curScore = GetWaterTerrainScore(pawn, c, curDist, false);
            if (!(maxScore < curScore))
            {
                continue;
            }

            isFound = true;
            maxScore = curScore;
            result = c;
        }

        return isFound;
    }

    public static int WillGetStackCountOf(Pawn getter, Thing thing)
    {
        var comp = thing.TryGetComp<CompWaterSource>();

        // 水源ではない→摂取数0
        if (comp is not { IsWaterSource: true })
        {
            return 0;
        }

        // アイテムではない→摂取数0
        if (comp.SourceType != CompProperties_WaterSource.SourceType.Item)
        {
            return 0;
        }

        // それを一度に摂取できる数と、何個摂取すれば水分が100%になるのか、の小さい方
        var wantedWaterItemCount = Math.Min(thing.TryGetComp<CompWaterSource>().MaxNumToGetAtOnce,
            StackCountForWater(thing, getter.needs.Water().WaterWanted));

        // 1個未満なら1個にする
        return wantedWaterItemCount < 1 ? 1 : wantedWaterItemCount;
    }

    private static Thing BestWaterInInventory(Pawn holder,
        WaterPreferability minWaterPref = WaterPreferability.SeaWater,
        WaterPreferability maxWaterPref = WaterPreferability.ClearWater, float minStackWaterAmount = 0.0f,
        bool allowDrug = false)
    {
        // 所持品から探すのに所持品オブジェクトなし
        if (holder?.inventory == null)
        {
            return null;
        }

        foreach (var thing in holder.inventory.innerContainer)
        {
            // 所持品をひとつずつチェック
            var waterAmount = thing.GetWaterAmount();
            var waterPreferability = thing.GetWaterPreferability();

            if (thing.CanGetWater() // 飲み物として飲めるもの
                && thing.CanDrinkWaterNow() // 現在飲める状態にある
                && !thing.def.IsIngestible // 食べ物ではない
                && waterPreferability >= minWaterPref && waterPreferability <= maxWaterPref // 品質が指定範囲内
                && (allowDrug || !thing.def.IsDrug) // ドラッグ許可か、そもそもドラッグでない
                && waterAmount * thing.stackCount >= minStackWaterAmount)
            {
                // 水の量の最低値指定を満たしている
                return thing;
            }
        }

        // 条件に合うものが1個も見つからなかった
        return null;
    }

    private static Thing BestWaterSourceOnMap(Pawn getter, Pawn eater, bool priorQuality,
        WaterPreferability maxPref = WaterPreferability.ClearWater, bool allowDrug = false,
        bool allowForbidden = false, bool allowSociallyImproper = false)
    {
        if (!getter.CanManipulate() && getter != eater)
        {
            // 取得者は操作不可、取得者と摂取者が違う
            // →マップから取得して持ち運ぶことができない
            // →エラー
            Log.Error($"{getter} tried to find food to bring to {eater} but {getter} is incapable of Manipulation.");
            return null;
        }

        if (getter.RaceProps.Humanlike)
        {
            // 取得者はHumanlikeである
            // →条件を満たすものの中から最適な物を探す
            return SpawnedWaterSearchInnerScan(
                eater,
                getter.Position,
                getter.Map.listerThings.ThingsInGroup(ThingRequestGroup.Everything).FindAll(
                    t =>
                    {
                        if (t.CanDrinkWaterNow())
                        {
                            return true;
                        }

                        return t is IBuilding_DrinkWater building && building.CanDrinkFor(eater);
                    }),
                PathEndMode.ClosestTouch,
                TraverseParms.For(getter),
                priorQuality,
                9999f,
                waterValidator);
        }

        // 取得者はHumanlikeではない

        // プレイヤー派閥に所属しているかどうかでリージョン?数を変える
        var searchRegionsMax = 30;
        if (getter.Faction == Faction.OfPlayer)
        {
            searchRegionsMax = 100;
        }

        var filtered = new HashSet<Thing>();
        foreach (var current in GenRadial.RadialDistinctThingsAround(getter.Position, getter.Map, 2f, true))
        {
            // 自分を中心に半径2以内の物をチェック
            if (current is Pawn pawn && pawn != getter && pawn.AnimalOrWildMan() && pawn.CurJob != null &&
                pawn.CurJob.def == MizuDef.Job_DrinkWater && pawn.CurJob.GetTarget(TargetIndex.A).HasThing)
            {
                // 自分ではない動物が現在水アイテムを摂取している
                // →今まさに摂取している物は探索対象から除外
                filtered.Add(pawn.CurJob.GetTarget(TargetIndex.A).Thing);
            }
        }

        var ignoreEntirelyForbiddenRegions = !allowForbidden // 禁止物のアクセスは許可されていない
                                             && ForbidUtility.CaresAboutForbidden(getter, true) &&
                                             getter.playerSettings?.EffectiveAreaRestrictionInPawnCurrentMap !=
                                             null; // 有効な制限エリアなし

        // 指定の条件下でアクセスできるものを探す

        // 水アイテムから
        var thing = GenClosest.ClosestThingReachable(getter.Position, getter.Map,
            ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.ClosestTouch,
            TraverseParms.For(getter), 9999f, predicate, null, 0, searchRegionsMax, false, RegionType.Set_Passable,
            ignoreEntirelyForbiddenRegions);
        if (thing != null)
        {
            return thing;
        }

        // 水汲み設備
        thing = GenClosest.ClosestThingReachable(getter.Position, getter.Map,
            ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.ClosestTouch,
            TraverseParms.For(getter), 9999f, predicate, null, 0, searchRegionsMax, false, RegionType.Set_Passable,
            ignoreEntirelyForbiddenRegions);
        if (thing != null)
        {
            return thing;
        }

        // 条件を変えて再探索
        // 水アイテム
        thing = GenClosest.ClosestThingReachable(
            getter.Position,
            getter.Map,
            ThingRequest.ForGroup(ThingRequestGroup.HaulableEver),
            PathEndMode.ClosestTouch,
            TraverseParms.For(getter),
            9999f,
            waterValidator, // ここが変わった
            null,
            0,
            searchRegionsMax,
            false,
            RegionType.Set_Passable,
            ignoreEntirelyForbiddenRegions);
        if (thing != null)
        {
            return thing;
        }

        // 水汲み設備
        thing = GenClosest.ClosestThingReachable(
            getter.Position,
            getter.Map,
            ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial),
            PathEndMode.ClosestTouch,
            TraverseParms.For(getter),
            9999f,
            waterValidator, // ここが変わった
            null,
            0,
            searchRegionsMax,
            false,
            RegionType.Set_Passable,
            ignoreEntirelyForbiddenRegions);
        return thing;

        bool predicate(Thing t)
        {
            // アイテムが条件を満たしていない
            if (!waterValidator(t))
            {
                return false;
            }

            // すぐ近くで他の動物が飲んでいる水のリストに入っていない
            if (filtered.Contains(t))
            {
                return false;
            }

            // 水の品質が最低値未満
            return t.GetWaterPreferability() >= WaterPreferability.SeaWater;
        }

        bool waterValidator(Thing t)
        {
            // 禁止されている＆禁止を無視して取得してはいけない
            if (!allowForbidden && t.IsForbidden(getter))
            {
                return false;
            }

            // ドラッグ禁止＆対象はドラッグ
            if (!allowDrug && t.def.IsDrug)
            {
                return false;
            }

            // 取得者が予約できない
            if (!getter.CanReserve(t))
            {
                return false;
            }

            var comp = t.TryGetComp<CompWaterSource>();

            // 水源として使用できない
            if (comp is not { IsWaterSource: true })
            {
                return false;
            }

            // 食べられるものは飲み物としては選ばれない
            if (t.def.IsIngestible)
            {
                return false;
            }

            // 操作が必要なのに操作できない
            if (comp.NeedManipulate && !getter.CanManipulate())
            {
                return false;
            }

            var waterTypeDef = MizuDef.Dic_WaterTypeDef[comp.WaterType];

            if (comp.SourceType == CompProperties_WaterSource.SourceType.Item)
            {
                // 水分がない
                if (!t.CanGetWater())
                {
                    return false;
                }

                // 水分を持っている=水アイテムである
                var waterPreferability = t.GetWaterPreferability();

                // 水の品質が範囲外
                if (waterPreferability < WaterPreferability.SeaWater || waterPreferability > maxPref)
                {
                    return false;
                }

                // 現在飲める状態には無い
                if (!t.CanDrinkWaterNow())
                {
                    return false;
                }

                // 入植者は囚人部屋のアイテムを扱えないことがあるが、そのことに関するチェックでダメならfalse
                return IsWaterSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper) &&
                       // 取得者がそれに気づいていない
                       getter.AnimalAwareOf(t);
            }

            if (comp.SourceType != CompProperties_WaterSource.SourceType.Building)
            {
                return false;
            }

            // 取得者と摂取者が異なる(自分で飲みに行く必要がある)
            if (getter != eater)
            {
                return false;
            }

            // 水汲みに使えない
            if (t is not IBuilding_DrinkWater drinkWaterBuilding)
            {
                return false;
            }

            // 水を飲む人が飲めない(能力が無い、水の量がない)
            if (!drinkWaterBuilding.CanDrinkFor(eater))
            {
                return false;
            }

            // 最大水質を超えていたらダメ
            if (waterTypeDef.waterPreferability > maxPref)
            {
                return false;
            }

            // 野生人?(派閥所属なし?)はダメ
            if (eater.IsWildMan())
            {
                return false;
            }

            // 自陣営or自陣営のホストの設備でなければダメ
            // 動物でない場合は、という条件を追加
            if (!eater.AnimalOrWildMan() && t.Faction != eater.Faction && t.Faction != eater.HostFaction)
            {
                return false;
            }

            // 使えない状態はダメ
            if (!drinkWaterBuilding.IsActivated)
            {
                return false;
            }

            // 入植者は囚人部屋のアイテムを扱えないことがあるが、そのことに関するチェックでダメならfalse
            if (!IsWaterSourceOnMapSociallyProper(t, getter, eater, allowSociallyImproper))
            {
                return false;
            }

            if (t.def.hasInteractionCell)
            {
                // 使用場所がある
                if (!t.InteractionCell.Standable(t.Map) || !eater.Map.reachability.CanReachNonLocal(getter.Position,
                        new TargetInfo(t.InteractionCell, t.Map), PathEndMode.OnCell,
                        TraverseParms.For(getter, Danger.Some)))
                {
                    // 使用場所に立てない or 使用場所まで行けない
                    return false;
                }
            }
            else
            {
                // 使用場所が無い
                if (!getter.Map.reachability.CanReachNonLocal(getter.Position, new TargetInfo(t.Position, t.Map),
                        PathEndMode.ClosestTouch, TraverseParms.For(getter, Danger.Some)))
                {
                    // その設備にタッチできない
                    return false;
                }
            }

            return true;

            // それ以外
        }
    }

    private static float GetWaterTerrainScore(Pawn eater, IntVec3 c, float dist, bool priorQuality)
    {
        var terrain = c.GetTerrain(eater.Map);

        // 水源ではない or 水源として使えない
        if (!terrain.IsWater())
        {
            return float.MinValue;
        }

        var waterTypeDef = MizuDef.Dic_WaterTypeDef[terrain.ToWaterType()];

        // 基本点計算

        // 距離
        var distScore = -dist;

        // 心情変化量(水質)
        // メモ
        // きれい= +10
        // 普通  =   0
        // 生水  =   0
        // 泥水  =  -6
        // 海水  =  -6
        var thoughtScore = 0f;

        // 禁欲の影響も含まれている
        var thoughtDefs = new List<ThoughtDef>();
        ThoughtsFromWaterTypeDef(eater, waterTypeDef, true, thoughtDefs);
        foreach (var thought in thoughtDefs)
        {
            thoughtScore += thought.stages[0].baseMoodEffect;
        }

        // 食中毒
        // メモ
        // きれい= 0    =>   0
        // 普通  = 0    =>   0
        // 生水  = 0.01 => -10
        // 泥水  = 0.03 => -30
        // 海水  = 0.03 => -30
        var foodPoisoningScore = -(waterTypeDef.foodPoisonChance * 1000f);

        // 健康悪化
        if (waterTypeDef.hediffs != null)
        {
        }

        // 基本点合計メモ
        // 心情,食中毒,健康,合計(禁欲)
        // きれい= +10,     0,   0, +10(   0)
        // 普通  =   0,     0,   0,   0(   0)
        // 生水  =   0,   -10,   0, -10( -10)
        // 泥水  =  -6,   -30,   0, -36( -30)
        // 海水  =  -6,   -30,-100,-136(-130)

        // 各種状態によるスコアの変化

        // 水質優先モードか否か
        if (priorQuality)
        {
            distScore /= 10f;
        }

        return distScore + thoughtScore + foodPoisoningScore;
    }

    private static WaterType GetWaterType(Thing t)
    {
        var compSource = t.TryGetComp<CompWaterSource>();

        // 水源ではない
        if (compSource == null)
        {
            return WaterType.NoWater;
        }

        // 建物、もしくはアイテムでも設定された水質を直接使用する場合
        if (compSource.SourceType == CompProperties_WaterSource.SourceType.Building ||
            compSource.DependIngredients == false)
        {
            return compSource.WaterType;
        }

        // 材料データ取得
        var compIngredients = t.TryGetComp<CompIngredients>();

        // 無ければ直接水質参照
        if (compIngredients == null)
        {
            return compSource.WaterType;
        }

        // 材料の中から最低の水質を取得
        var waterType = WaterType.NoWater;
        foreach (var ingredient in compIngredients.ingredients)
        {
            var comp = ingredient.GetCompProperties<CompProperties_WaterSource>();
            if (comp == null)
            {
                continue;
            }

            waterType = waterType.GetMinType(comp.waterType);
        }

        return waterType;
    }

    private static bool IsWaterSourceOnMapSociallyProper(Thing t, Pawn getter, Pawn eater,
        bool allowSociallyImproper)
    {
        // 囚人部屋にあっても強引に使用して良い
        if (allowSociallyImproper)
        {
            return true;
        }

        // 適切な場所にある
        return t.IsSociallyProper(getter) ||
               t.IsSociallyProper(eater, eater.IsPrisonerOfColony, !getter.AnimalOrWildMan());
    }

    private static Thing SpawnedWaterSearchInnerScan(Pawn eater, IntVec3 root, List<Thing> searchSet,
        PathEndMode peMode, TraverseParms traverseParams, bool priorQuality, float maxDistance = 9999f,
        Predicate<Thing> validator = null)
    {
        // 探索対象リストなし
        if (searchSet == null)
        {
            return null;
        }

        // 対象のポーンを決める(取得者優先、次点で摂取者)
        var pawn = traverseParams.pawn ?? eater;

        Thing result = null;
        var maxScore = float.MinValue;

        foreach (var thing in searchSet)
        {
            // アイテムとの距離が限界以上離れていたらダメ
            var lengthManhattan = (float)(root - thing.Position).LengthManhattan;
            if (lengthManhattan > maxDistance)
            {
                continue;
            }

            // 現時点での候補アイテムのスコア(摂取者にとって)を超えていないならダメ
            var thingScore = GetWaterItemScore(eater, thing, lengthManhattan, priorQuality);
            if (thingScore < maxScore)
            {
                continue;
            }

            // ポーンがそこまでたどり着けなければだめ
            if (!pawn.Map.reachability.CanReach(root, thing, peMode, traverseParams))
            {
                continue;
            }

            // まだ出現していない場合はダメ
            if (!thing.Spawned)
            {
                continue;
            }

            // アイテムが指定の条件を満たしていないならダメ
            if (validator != null && !validator(thing))
            {
                continue;
            }

            // すべての条件を満足
            result = thing;
            maxScore = thingScore;
        }

        return result;
    }
}