using HarmonyLib;
using RimWorld;

namespace MizuMod
{
    [HarmonyPatch(typeof(Dialog_Trade))]
    [HarmonyPatch("CountToTransferChanged")]
    internal class Dialog_Trade_CountToTransferChanged
    {
        private static void Postfix()
        {
            MizuCaravanUtility.daysWorthOfWaterDirty = true;
        }
    }
}