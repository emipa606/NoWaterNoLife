using HarmonyLib;
using RimWorld;

namespace MizuMod.HarmonyPatches;

[HarmonyPatch(typeof(Dialog_LoadTransporters), "CalculateAndRecacheTransferables")]
internal class Dialog_LoadTransporters_CountToTransferChanged
{
    private static void Postfix()
    {
        MizuCaravanUtility.daysWorthOfWaterDirty = true;
    }
}