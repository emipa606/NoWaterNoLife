using RimWorld;
using Verse;

namespace MizuMod;

public class RecipeWorkerCounter_DrawWater : RecipeWorkerCounter
{
    public GetWaterRecipeDef GetWaterRecipe => (GetWaterRecipeDef)recipe;

    public override bool CanCountProducts(Bill_Production bill)
    {
        var ext = bill.recipe.GetModExtension<DefExtension_WaterRecipe>();
        if (ext == null)
        {
            return false;
        }

        return bill.billStack.billGiver is IBuilding_DrinkWater;
    }

    public override int CountProducts(Bill_Production bill)
    {
        // var ext = bill.recipe.GetModExtension<DefExtension_WaterRecipe>();
        if (bill.billStack.billGiver is not IBuilding_DrinkWater building)
        {
            return 0;
        }

        var waterDef = MizuUtility.GetWaterThingDefFromWaterType(building.WaterType);
        if (waterDef == null)
        {
            return 0;
        }

        var numOfWater = bill.Map.resourceCounter.GetCount(waterDef);

        return numOfWater;
    }

    public override string ProductsDescription(Bill_Production bill)
    {
        return null;
    }
}