using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MizuMod
{
    [HarmonyPatch(typeof(Dialog_Trade))]
    [HarmonyPatch("DoWindowContents")]
    internal class Dialog_Trade_DoWindowContents
    {
        private static void Prefix(List<Thing> ___playerCaravanAllPawnsAndItems, List<Tradeable> ___cachedTradeables)
        {
            if (___playerCaravanAllPawnsAndItems != null)
            {
                MizuCaravanUtility.DaysWorthOfWater_Trade(___playerCaravanAllPawnsAndItems, ___cachedTradeables);
            }
        }
    }
}