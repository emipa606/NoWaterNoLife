using System.Collections.Generic;
using HarmonyLib;
using RimWorld;

namespace MizuMod
{
    [HarmonyPatch(typeof(Dialog_LoadTransporters))]
    [HarmonyPatch("DoWindowContents")]
    internal class Dialog_LoadTransporters_DoWindowContents
    {
        private static void Prefix(List<TransferableOneWay> ___transferables)
        {
            MizuCaravanUtility.DaysWorthOfWater_LoadTransporters(___transferables);
        }
    }
}