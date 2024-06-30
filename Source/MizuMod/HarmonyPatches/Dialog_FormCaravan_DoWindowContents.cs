using HarmonyLib;
using RimWorld;

namespace MizuMod.HarmonyPatches;

[HarmonyPatch(typeof(Dialog_FormCaravan), nameof(Dialog_FormCaravan.DoWindowContents))]
internal class Dialog_FormCaravan_DoWindowContents
{
    private static void Prefix(Dialog_FormCaravan __instance)
    {
        MizuCaravanUtility.DaysWorthOfWater_FormCaravan(__instance);
    }
}