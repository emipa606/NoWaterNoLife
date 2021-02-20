using UnityEngine;
using Verse;

namespace MizuMod
{
    public class JobDriver_DrawFromWaterPool : JobDriver_DrawWater
    {
        private UndergroundWaterPool poolInt;
        private MapComponent_ShallowWaterGrid waterGridInt;

        private MapComponent_ShallowWaterGrid WaterGrid
        {
            get
            {
                if (waterGridInt == null)
                {
                    waterGridInt = TargetThingA.Map.GetComponent<MapComponent_ShallowWaterGrid>();
                }

                return waterGridInt;
            }
        }

        private UndergroundWaterPool Pool
        {
            get
            {
                if (poolInt == null)
                {
                    poolInt = WaterGrid.GetPool(
                        TargetThingA.Map.cellIndices.CellToIndex(job.GetTarget(BillGiverInd).Thing.Position));
                }

                return poolInt;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!base.TryMakePreToilReservations(errorOnFailed))
            {
                return false;
            }

            if (WaterGrid == null)
            {
                return false;
            }

            if (Pool == null)
            {
                return false;
            }

            return true;
        }

        protected override void SetFailCondition()
        {
        }

        protected override Thing FinishAction()
        {
            // 地下水脈の水の種類から水アイテムの種類を決定
            var waterThingDef = MizuUtility.GetWaterThingDefFromWaterType(Pool.WaterType);

            // 水アイテムの水源情報を得る
            var compprop = waterThingDef?.GetCompProperties<CompProperties_WaterSource>();
            if (compprop == null)
            {
                return null;
            }

            // 地下水脈から水を減らす
            Pool.CurrentWaterVolume = Mathf.Max(0, Pool.CurrentWaterVolume - (compprop.waterVolume * Ext.getItemCount));

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

        //public override string GetReport()
        //{
        //    return base.GetReport() + "!!!" + this.CurToilIndex.ToString();
        //}
    }
}