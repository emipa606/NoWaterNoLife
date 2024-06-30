using HarmonyLib;
using RimWorld;

namespace MizuMod.HarmonyPatches;

[HarmonyPatch(typeof(Dialog_Trade), "CountToTransferChanged")]
internal class Dialog_Trade_CountToTransferChanged
{
    private static void Postfix()
    {
        MizuCaravanUtility.daysWorthOfWaterDirty = true;
    }
}