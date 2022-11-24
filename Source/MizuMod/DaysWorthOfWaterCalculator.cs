using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MizuMod;

public static class DaysWorthOfWaterCalculator
{
    private static readonly List<Pawn> tmpPawns = new List<Pawn>();

    private static readonly List<ThingDefCount> tmpThingDefCounts = new List<ThingDefCount>();

    private static readonly List<ThingCount> tmpThingStackParts = new List<ThingCount>();

    public static float ApproxDaysWorthOfWater(
        List<TransferableOneWay> transferables,
        IgnorePawnsInventoryMode ignoreInventory)
    {
        tmpThingDefCounts.Clear();
        tmpPawns.Clear();

        foreach (var transferableOneWay in transferables)
        {
            if (!transferableOneWay.HasAnyThing)
            {
                continue;
            }

            if (transferableOneWay.AnyThing is Pawn)
            {
                for (var j = 0; j < transferableOneWay.CountToTransfer; j++)
                {
                    tmpPawns.Add((Pawn)transferableOneWay.things[j]);
                }
            }
            else
            {
                tmpThingDefCounts.Add(
                    new ThingDefCount(transferableOneWay.ThingDef, transferableOneWay.CountToTransfer));
            }
        }

        var result = ApproxDaysWorthOfWater(tmpPawns, tmpThingDefCounts, ignoreInventory);
        tmpThingDefCounts.Clear();
        tmpPawns.Clear();
        return result;
    }

    public static float ApproxDaysWorthOfWater(Caravan caravan)
    {
        return ApproxDaysWorthOfWater(caravan.PawnsListForReading, null, IgnorePawnsInventoryMode.DontIgnore);
    }

    public static float ApproxDaysWorthOfWaterLeftAfterTradeableTransfer(
        List<Thing> allCurrentThings,
        List<Tradeable> tradeables,
        IgnorePawnsInventoryMode ignoreInventory)
    {
        TransferableUtility.SimulateTradeableTransfer(allCurrentThings, tradeables, tmpThingStackParts);
        tmpPawns.Clear();
        tmpThingDefCounts.Clear();
        for (var i = tmpThingStackParts.Count - 1; i >= 0; i--)
        {
            if (tmpThingStackParts[i].Thing is Pawn pawn)
            {
                tmpPawns.Add(pawn);
            }
            else
            {
                tmpThingDefCounts.Add(
                    new ThingDefCount(tmpThingStackParts[i].Thing.def, tmpThingStackParts[i].Count));
            }
        }

        tmpThingStackParts.Clear();
        var result = ApproxDaysWorthOfWater(tmpPawns, tmpThingDefCounts, ignoreInventory);
        tmpPawns.Clear();
        tmpThingDefCounts.Clear();
        return result;
    }

    private static bool AnyNonTerrainDrinkingPawn(List<Pawn> pawns)
    {
        foreach (var pawn in pawns)
        {
            if (pawn.needs.mood != null && pawn.needs.Water() != null)
            {
                return true;
            }
        }

        return false;
    }

    private static float ApproxDaysWorthOfWater(
        List<Pawn> pawns,
        List<ThingDefCount> extraWater,
        IgnorePawnsInventoryMode ignoreInventory)
    {
        if (!AnyNonTerrainDrinkingPawn(pawns))
        {
            return 1000f;
        }

        var tmpWater = new List<ThingDefCount>();
        tmpWater.Clear();

        if (extraWater != null)
        {
            foreach (var thingDefCount in extraWater)
            {
                var canGetWater = false;
                foreach (var compProperties in thingDefCount.ThingDef.comps)
                {
                    if (compProperties is not CompProperties_WaterSource
                        {
                            sourceType: CompProperties_WaterSource.SourceType.Item, waterAmount: > 0.0f
                        })
                    {
                        continue;
                    }

                    canGetWater = true;
                    break;
                }

                if (canGetWater && thingDefCount.Count > 0)
                {
                    tmpWater.Add(thingDefCount);
                }
            }
        }

        foreach (var pawn in pawns)
        {
            if (InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignoreInventory))
            {
                continue;
            }

            var innerContainer = pawn.inventory.innerContainer;
            foreach (var thing in innerContainer)
            {
                if (thing.CanGetWater())
                {
                    tmpWater.Add(new ThingDefCount(thing.def, thing.stackCount));
                }
            }
        }

        if (!tmpWater.Any())
        {
            return 0f;
        }

        var tmpDaysWorthOfFoodPerPawn = new List<float>();
        var tmpAnyFoodLeftIngestibleByPawn = new List<bool>();
        tmpDaysWorthOfFoodPerPawn.Clear();
        tmpAnyFoodLeftIngestibleByPawn.Clear();
        for (var l = 0; l < pawns.Count; l++)
        {
            tmpDaysWorthOfFoodPerPawn.Add(0f);
            tmpAnyFoodLeftIngestibleByPawn.Add(true);
        }

        var num = 0f;
        bool b;
        do
        {
            b = false;
            for (var m = 0; m < pawns.Count; m++)
            {
                var pawn = pawns[m];
                if (!tmpAnyFoodLeftIngestibleByPawn[m])
                {
                    continue;
                }

                do
                {
                    var num2 = BestEverGetWaterIndexFor(tmpWater);
                    if (num2 < 0)
                    {
                        tmpAnyFoodLeftIngestibleByPawn[m] = false;
                        break;
                    }

                    CompProperties_WaterSource compprop = null;
                    foreach (var compProperties in tmpWater[num2].ThingDef.comps)
                    {
                        compprop = compProperties as CompProperties_WaterSource;
                        if (compprop is { sourceType: CompProperties_WaterSource.SourceType.Item })
                        {
                            break;
                        }
                    }

                    if (compprop == null)
                    {
                        tmpAnyFoodLeftIngestibleByPawn[m] = false;
                        break;
                    }

                    var need_water = pawn.needs.Water();
                    if (need_water == null)
                    {
                        tmpAnyFoodLeftIngestibleByPawn[m] = false;
                        break;
                    }

                    var num3 = Mathf.Min(compprop.waterAmount, need_water.WaterAmountBetweenThirstyAndHealthy);
                    var num4 = num3 / need_water.WaterAmountBetweenThirstyAndHealthy
                        * need_water.TicksUntilThirstyWhenHealthy / 60000f;
                    tmpDaysWorthOfFoodPerPawn[m] = tmpDaysWorthOfFoodPerPawn[m] + num4;
                    tmpWater[num2] = tmpWater[num2].WithCount(tmpWater[num2].Count - 1);
                    b = true;
                } while (tmpDaysWorthOfFoodPerPawn[m] < num);

                num = Mathf.Max(num, tmpDaysWorthOfFoodPerPawn[m]);
            }
        } while (b);

        var num6 = 1000f;
        for (var n = 0; n < pawns.Count; n++)
        {
            num6 = Mathf.Min(num6, tmpDaysWorthOfFoodPerPawn[n]);
        }

        return num6;
    }

    private static int BestEverGetWaterIndexFor(List<ThingDefCount> water)
    {
        var num = -1;
        var num2 = 0f;
        for (var i = 0; i < water.Count; i++)
        {
            if (water[i].Count <= 0)
            {
                continue;
            }

            var thingDef = water[i].ThingDef;
            if (!MizuCaravanUtility.CanEverGetWater(thingDef))
            {
                continue;
            }

            var waterScore = MizuCaravanUtility.GetWaterScore(thingDef);
            if (num >= 0 && !(waterScore > num2))
            {
                continue;
            }

            num = i;
            num2 = waterScore;
        }

        return num;
    }
}