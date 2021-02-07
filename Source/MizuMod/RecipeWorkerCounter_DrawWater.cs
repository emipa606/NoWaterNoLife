using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
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

            if (!(bill.billStack.billGiver is IBuilding_DrinkWater building))
            {
                return false;
            }

            return true;
        }

        public override int CountProducts(Bill_Production bill)
        {
            var ext = bill.recipe.GetModExtension<DefExtension_WaterRecipe>();
            var building = bill.billStack.billGiver as IBuilding_DrinkWater;

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
}
