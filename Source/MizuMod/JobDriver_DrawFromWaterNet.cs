using Verse;

namespace MizuMod
{
    public class JobDriver_DrawFromWaterNet : JobDriver_DrawWater
    {
        private WaterNet WaterNet => WorkTable?.InputWaterNet;

        private Building_WaterNetWorkTable WorkTable => job.GetTarget(BillGiverInd).Thing as Building_WaterNetWorkTable;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!base.TryMakePreToilReservations(errorOnFailed))
            {
                return false;
            }

            if (WorkTable == null)
            {
                return false;
            }

            if (WaterNet == null)
            {
                return false;
            }

            return true;
        }

        protected override Thing FinishAction()
        {
            var targetWaterType = Ext.canDrawFromFaucet
                                      ? WaterNet.StoredWaterTypeForFaucet
                                      : WorkTable.TankComp.StoredWaterType;

            // 水道網の水の種類から水アイテムの種類を決定
            var waterThingDef = MizuUtility.GetWaterThingDefFromWaterType(targetWaterType);

            // 水アイテムの水源情報を得る
            var compprop = waterThingDef?.GetCompProperties<CompProperties_WaterSource>();
            if (compprop == null)
            {
                return null;
            }

            // 水道網から水を減らす
            if (Ext.canDrawFromFaucet)
            {
                // 蛇口の場合
                WaterNet.DrawWaterVolumeForFaucet(compprop.waterVolume * Ext.getItemCount);
            }
            else
            {
                // 自分自身の場合
                WorkTable.TankComp.DrawWaterVolume(compprop.waterVolume * Ext.getItemCount);
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