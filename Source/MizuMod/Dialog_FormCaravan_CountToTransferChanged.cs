using HarmonyLib;
using RimWorld;

namespace MizuMod
{
    [HarmonyPatch(typeof(Dialog_FormCaravan))]
    [HarmonyPatch("CountToTransferChanged")]
    internal class Dialog_FormCaravan_CountToTransferChanged
    {
        private static void Postfix()
        {
            MizuCaravanUtility.daysWorthOfWaterDirty = true;
        }
    }
}