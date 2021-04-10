using HarmonyLib;
using RimWorld;

namespace MizuMod
{
    [HarmonyPatch(typeof(Dialog_FormCaravan))]
    [HarmonyPatch("DoWindowContents")]
    internal class Dialog_FormCaravan_DoWindowContents
    {
        private static void Prefix(Dialog_FormCaravan __instance)
        {
            MizuCaravanUtility.DaysWorthOfWater_FormCaravan(__instance);
        }
    }
}