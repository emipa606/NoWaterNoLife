using System.Collections.Generic;
using Verse;

namespace MizuMod
{
    public class DefExtension_WaterRecipe : DefModExtension
    {
        public enum RecipeType : byte
        {
            Undefined = 0,

            DrawFromTerrain,

            DrawFromWaterPool,

            DrawFromWaterNet,

            PourWater
        }

        public bool canDrawFromFaucet = false;

        public int getItemCount = 1;

        public List<WaterTerrainType> needWaterTerrainTypes = null;

        public List<WaterType> needWaterTypes = null;

        public RecipeType recipeType = RecipeType.Undefined;
    }
}