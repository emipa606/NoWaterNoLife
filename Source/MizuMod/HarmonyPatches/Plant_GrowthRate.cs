using HarmonyLib;
using RimWorld;

namespace MizuMod.HarmonyPatches;

[HarmonyPatch(typeof(Plant), nameof(Plant.GrowthRate), MethodType.Getter)]
internal class Plant_GrowthRate
{
    private static void Postfix(Plant __instance, ref float __result)
    {
        var map = __instance.Map;
        int wateringRemainTicks = map.GetComponent<MapComponent_Watering>()
            .Get(map.cellIndices.CellToIndex(__instance.Position));
        if (wateringRemainTicks > 0)
        {
            // 水やりされている
            __result *= MizuModBody.Settings.FertilityFactorInWatering;
        }
        else
        {
            // 水やりされていない
            __result *= MizuModBody.Settings.FertilityFactorInNotWatering;
        }
    }
}