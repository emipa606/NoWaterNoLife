using HarmonyLib;
using RimWorld;

namespace MizuMod;

[HarmonyPatch(typeof(Dialog_LoadTransporters))]
[HarmonyPatch("CountToTransferChanged")]
internal class Dialog_LoadTransporters_CountToTransferChanged
{
    private static void Postfix()
    {
        MizuCaravanUtility.daysWorthOfWaterDirty = true;
    }
}