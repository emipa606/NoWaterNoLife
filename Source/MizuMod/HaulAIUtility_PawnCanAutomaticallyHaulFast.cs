using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    [HarmonyPatch(typeof(HaulAIUtility))]
    [HarmonyPatch("PawnCanAutomaticallyHaulFast")]
    internal class HaulAIUtility_PawnCanAutomaticallyHaulFast
    {
        private static void Postfix(ref bool __result, Pawn p, Thing t)
        {
            if (!t.CanGetWater() || t.IsSociallyProper(p, false, true))
            {
                return;
            }

            JobFailReason.Is("ReservedForPrisoners".Translate());
            __result = false;
        }
    }
}