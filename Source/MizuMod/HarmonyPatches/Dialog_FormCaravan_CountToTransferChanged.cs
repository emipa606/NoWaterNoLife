using HarmonyLib;
using RimWorld;

namespace MizuMod.HarmonyPatches;

[HarmonyPatch(typeof(Dialog_FormCaravan), "CalculateAndRecacheTransferables")]
internal class Dialog_FormCaravan_CountToTransferChanged
{
    private static void Postfix()
    {
        MizuCaravanUtility.daysWorthOfWaterDirty = true;
    }
}