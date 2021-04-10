using System.Collections.Generic;
using Verse;

namespace MizuMod
{
    public class GetWaterRecipeDef : RecipeDef
    {
        // public float needWaterVolume = 1.0f;
        public int getItemCount = 1;

        public List<WaterTerrainType> needWaterTerrainTypes = null;

        public List<WaterType> needWaterTypes = null;

        public override void PostLoad()
        {
            base.PostLoad();

            if (products != null)
            {
                return;
            }

            var thingCountClass = new ThingDefCountClass();
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(thingCountClass, "thingDef", "Mizu_NormalWater");
            thingCountClass.count = 1;

            products = new List<ThingDefCountClass> { thingCountClass };
        }
    }
}