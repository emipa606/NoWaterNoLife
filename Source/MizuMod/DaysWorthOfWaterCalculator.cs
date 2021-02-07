using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MizuMod
{
    public static class DaysWorthOfWaterCalculator
    {
        public static float ApproxDaysWorthOfWater(List<TransferableOneWay> transferables, IgnorePawnsInventoryMode ignoreInventory)
        {
            tmpThingDefCounts.Clear();
            tmpPawns.Clear();

            for (var i = 0; i < transferables.Count; i++)
            {
                TransferableOneWay transferableOneWay = transferables[i];
                if (transferableOneWay.HasAnyThing)
                {
                    if (transferableOneWay.AnyThing is Pawn)
                    {
                        for (var j = 0; j < transferableOneWay.CountToTransfer; j++)
                        {
                            tmpPawns.Add((Pawn)transferableOneWay.things[j]);
                        }
                    }
                    else
                    {
                    	tmpThingDefCounts.Add(new ThingDefCount(transferableOneWay.ThingDef, transferableOneWay.CountToTransfer));
                    }
                }
            }
            var result = DaysWorthOfWaterCalculator.ApproxDaysWorthOfWater(tmpPawns, tmpThingDefCounts, ignoreInventory);
            tmpThingDefCounts.Clear();
            tmpPawns.Clear();
            return result;
        }

        private static bool AnyNonTerrainDrinkingPawn(List<Pawn> pawns)
        {
            for (var i = 0; i < pawns.Count; i++)
            {
                if (pawns[i].needs.mood != null && pawns[i].needs.Water() != null)
                {
                    return true;
                }
            }
            return false;
        }

        public static float ApproxDaysWorthOfWater(Caravan caravan)
        {
            return DaysWorthOfWaterCalculator.ApproxDaysWorthOfWater(caravan.PawnsListForReading, null, IgnorePawnsInventoryMode.DontIgnore);
        }

        private static float ApproxDaysWorthOfWater(List<Pawn> pawns, List<ThingDefCount> extraWater, IgnorePawnsInventoryMode ignoreInventory)
        {
            if (!DaysWorthOfWaterCalculator.AnyNonTerrainDrinkingPawn(pawns))
            {
                return 1000f;
            }
            var tmpWater = new List<ThingDefCount>();
            tmpWater.Clear();

            if (extraWater != null)
            {

                for (var i = 0; i < extraWater.Count; i++)
                {
                    var canGetWater = false;
                    for (var j = 0; j < extraWater[i].ThingDef.comps.Count; j++)
                    {
                        if (extraWater[i].ThingDef.comps[j] is CompProperties_WaterSource compprop && compprop.sourceType == CompProperties_WaterSource.SourceType.Item && compprop.waterAmount > 0.0f)
                        {
                            canGetWater = true;
                            break;
                        }
                    }
                    if (canGetWater && extraWater[i].Count > 0)
                    {
                        tmpWater.Add(extraWater[i]);
                    }
                }

            }
            for (var j = 0; j < pawns.Count; j++)
            {
                if (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawns[j], ignoreInventory))
                {
                    ThingOwner<Thing> innerContainer = pawns[j].inventory.innerContainer;
                    for (var k = 0; k < innerContainer.Count; k++)
                    {
                        if (innerContainer[k].CanGetWater())
                        {
                            tmpWater.Add(new ThingDefCount(innerContainer[k].def, innerContainer[k].stackCount));
                        }
                    }
                }
            }
            if (!tmpWater.Any<ThingDefCount>())
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
            bool flag;
            do
            {
                flag = false;
                for (var m = 0; m < pawns.Count; m++)
                {
                    Pawn pawn = pawns[m];
                    if (tmpAnyFoodLeftIngestibleByPawn[m])
                    {
                        do
                        {
                            var num2 = DaysWorthOfWaterCalculator.BestEverGetWaterIndexFor(pawns[m], tmpWater);
                            if (num2 < 0)
                            {
                                tmpAnyFoodLeftIngestibleByPawn[m] = false;
                                break;
                            }
                            CompProperties_WaterSource compprop = null;
                            for (var x = 0; x < tmpWater[num2].ThingDef.comps.Count; x++)
                            {
                                compprop = tmpWater[num2].ThingDef.comps[x] as CompProperties_WaterSource;
                                if (compprop != null && compprop.sourceType == CompProperties_WaterSource.SourceType.Item)
                                {
                                    break;
                                }
                            }
                            if (compprop == null)
                            {
                                tmpAnyFoodLeftIngestibleByPawn[m] = false;
                                break;
                            }
                            Need_Water need_water = pawn.needs.Water();
                            if (need_water == null)
                            {
                                tmpAnyFoodLeftIngestibleByPawn[m] = false;
                                break;
                            }
                            var num3 = Mathf.Min(compprop.waterAmount, need_water.WaterAmountBetweenThirstyAndHealthy);
                            var num4 = num3 / need_water.WaterAmountBetweenThirstyAndHealthy * (float)need_water.TicksUntilThirstyWhenHealthy / 60000f;
                            tmpDaysWorthOfFoodPerPawn[m] = tmpDaysWorthOfFoodPerPawn[m] + num4;
                            tmpWater[num2] = tmpWater[num2].WithCount(tmpWater[num2].Count - 1);
                            flag = true;
                        }
                        while (tmpDaysWorthOfFoodPerPawn[m] < num);
                        num = Mathf.Max(num, tmpDaysWorthOfFoodPerPawn[m]);
                    }
                }
            }
            while (flag);
            var num6 = 1000f;
            for (var n = 0; n < pawns.Count; n++)
            {
                num6 = Mathf.Min(num6, tmpDaysWorthOfFoodPerPawn[n]);
            }
            return num6;
        }

        private static readonly List<Pawn> tmpPawns = new List<Pawn>();
        private static readonly List<ThingCount> tmpThingCounts = new List<ThingCount>();
        private static readonly List<ThingCount> tmpThingStackParts = new List<ThingCount>();
        private static readonly List<ThingDefCount> tmpThingDefCounts = new List<ThingDefCount>();

        public static float ApproxDaysWorthOfWaterLeftAfterTradeableTransfer(List<Thing> allCurrentThings, List<Tradeable> tradeables, IgnorePawnsInventoryMode ignoreInventory)
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
                    tmpThingDefCounts.Add(new ThingDefCount(tmpThingStackParts[i].Thing.def, tmpThingStackParts[i].Count));
                }
            }
            tmpThingStackParts.Clear();
            var result = ApproxDaysWorthOfWater(tmpPawns, tmpThingDefCounts, ignoreInventory);
            tmpPawns.Clear();
            tmpThingDefCounts.Clear();
            return result;
        }

        private static int BestEverGetWaterIndexFor(Pawn pawn, List<ThingDefCount> water)
        {
            var num = -1;
            var num2 = 0f;
            for (var i = 0; i < water.Count; i++)
            {
                if (water[i].Count > 0)
                {
                    ThingDef thingDef = water[i].ThingDef;
                    if (MizuCaravanUtility.CanEverGetWater(thingDef, pawn))
                    {
                        var waterScore = MizuCaravanUtility.GetWaterScore(thingDef, pawn);
                        if (num < 0 || waterScore > num2)
                        {
                            num = i;
                            num2 = waterScore;
                        }
                    }
                }
            }
            return num;
        }
    }
}
