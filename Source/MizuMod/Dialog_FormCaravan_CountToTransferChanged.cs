using HarmonyLib;
using RimWorld;

namespace MizuMod;

[HarmonyPatch(typeof(Dialog_FormCaravan))]
[HarmonyPatch("CalculateAndRecacheTransferables")]
internal class Dialog_FormCaravan_CountToTransferChanged
{
    private static void Postfix()
    {
        MizuCaravanUtility.daysWorthOfWaterDirty = true;
    }
}