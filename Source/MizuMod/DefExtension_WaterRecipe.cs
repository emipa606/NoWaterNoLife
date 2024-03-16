using System.Collections.Generic;
using Verse;

namespace MizuMod;

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

    public readonly bool canDrawFromFaucet = false;

    public readonly int getItemCount = 1;

    public readonly List<WaterTerrainType> needWaterTerrainTypes = null;

    public readonly List<WaterType> needWaterTypes = null;

    public readonly RecipeType recipeType = RecipeType.Undefined;
}