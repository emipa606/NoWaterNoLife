using HarmonyLib;
using RimWorld;

namespace MizuMod
{
    [HarmonyPatch(typeof(Pawn_NeedsTracker))]
    [HarmonyPatch("ShouldHaveNeed")]
    internal class Pawn_NeedsTracker_ShouldHaveNeed
    {
        private static void Postfix(Pawn_NeedsTracker __instance, NeedDef nd, ref bool __result)
        {
            if (nd != MizuDef.Need_Water)
            {
                return;
            }

            if (__instance.food == null)
            {
                __result = false;
            }
        }
    }
}