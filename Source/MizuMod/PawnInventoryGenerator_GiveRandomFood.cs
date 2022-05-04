using HarmonyLib;
using RimWorld;
using Verse;

namespace MizuMod;

[HarmonyPatch(typeof(PawnInventoryGenerator))]
[HarmonyPatch("GiveRandomFood")]
internal class PawnInventoryGenerator_GiveRandomFood
{
    private static void Postfix(Pawn p)
    {
        if (p.kindDef.invNutrition <= 0.001f)
        {
            return;
        }

        ThingDef thingDef;
        if (p.kindDef.itemQuality > QualityCategory.Normal)
        {
            thingDef = MizuDef.Thing_ClearWater;
        }
        else if (p.kindDef.itemQuality == QualityCategory.Normal)
        {
            thingDef = MizuDef.Thing_NormalWater;
        }
        else
        {
            var value = Rand.Value;
            if (value < 0.7f)
            {
                // 70%
                thingDef = MizuDef.Thing_RawWater;
            }
            else if (value < 0.9)
            {
                // 20%
                thingDef = MizuDef.Thing_MudWater;
            }
            else
            {
                // 10%
                thingDef = MizuDef.Thing_SeaWater;
            }
        }

        var compprop = thingDef.GetCompProperties<CompProperties_WaterSource>();
        if (compprop == null)
        {
            return;
        }

        var thing = ThingMaker.MakeThing(thingDef);
        thing.stackCount = GenMath.RoundRandom(p.kindDef.invNutrition / compprop.waterAmount);

        p.inventory.TryAddItemNotForSale(thing);
    }
}