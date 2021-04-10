using Verse;

namespace MizuMod
{
    public class JobDriver_DrawFromTerrain : JobDriver_DrawWater
    {
        protected override Thing FinishAction()
        {
            // 現在の地形から水の種類を決定
            var terrainDef = Map.terrainGrid.TerrainAt(job.GetTarget(BillGiverInd).Thing.Position);
            var waterTerrainType = terrainDef.GetWaterTerrainType();
            var waterThingDef = MizuUtility.GetWaterThingDefFromTerrainType(waterTerrainType);
            if (waterThingDef == null)
            {
                return null;
            }

            // 水を生成
            var createThing = ThingMaker.MakeThing(waterThingDef);
            if (createThing == null)
            {
                return null;
            }

            // 個数設定
            createThing.stackCount = Ext.getItemCount;
            return createThing;
        }

        protected override void SetFailCondition()
        {
        }
    }
}