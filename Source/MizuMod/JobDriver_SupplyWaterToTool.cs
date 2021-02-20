using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class JobDriver_SupplyWaterToTool : JobDriver
    {
        private const TargetIndex SourceInd = TargetIndex.A;
        private const TargetIndex ToolInd = TargetIndex.B;
        private const TargetIndex StoreToolPosInd = TargetIndex.C;

        private float maxTick;
        private bool needManipulate;

        private ThingWithComps SourceThing => (ThingWithComps) job.GetTarget(SourceInd).Thing;
        private ThingWithComps Tool => (ThingWithComps) job.GetTarget(ToolInd).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.Reserve(SourceThing, job);
            pawn.Reserve(Tool, job);
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // 水ツールを手に取る
            yield return Toils_Goto.GotoThing(ToolInd, PathEndMode.Touch);
            yield return Toils_Haul.StartCarryThing(ToolInd);

            // 水汲み設備へ移動
            var peMode = PathEndMode.ClosestTouch;
            if (SourceThing.def.hasInteractionCell)
            {
                peMode = PathEndMode.InteractionCell;
            }

            yield return Toils_Goto.GotoThing(SourceInd, peMode);

            // 水汲み
            var supplyToil = new Toil
            {
                initAction = () =>
                {
                    var compSource = SourceThing.GetComp<CompWaterSource>();
                    needManipulate = compSource.NeedManipulate;

                    // 水汲み速度関連はリファクタリングしたい
                    var ticksForFull = compSource.BaseDrinkTicks;

                    var compTool = Tool.GetComp<CompWaterTool>();
                    var totalTicks = (int) (ticksForFull * (1f - compTool.StoredWaterVolumePercent));
                    if (!needManipulate)
                    {
                        // 手が必要ない→水にドボンですぐに補給できる
                        totalTicks /= 10;
                    }

                    // 小数の誤差を考慮して1Tick余分に多く実行する
                    totalTicks += 1;

                    maxTick = totalTicks;
                    ticksLeftThisToil = totalTicks;
                },
                tickAction = () =>
                {
                    var compSource = SourceThing.GetComp<CompWaterSource>();
                    var compTool = Tool.GetComp<CompWaterTool>();
                    var building = SourceThing as IBuilding_DrinkWater;

                    var supplyWaterVolume = compTool.MaxWaterVolume / compSource.BaseDrinkTicks;
                    if (!needManipulate)
                    {
                        supplyWaterVolume *= 10;
                    }

                    compTool.StoredWaterVolume += supplyWaterVolume;
                    if (building == null)
                    {
                        return;
                    }

                    compTool.StoredWaterType = building.WaterType;

                    building.DrawWater(supplyWaterVolume);

                    if (building.IsEmpty)
                    {
                        ReadyForNextToil();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Delay
            };
            supplyToil.WithProgressBar(SourceInd, () => 1f - (ticksLeftThisToil / maxTick), true);
            supplyToil.EndOnDespawnedOrNull(SourceInd);
            yield return supplyToil;

            // 水ツールを戻す
            yield return Toils_Mizu.TryFindStoreCell(ToolInd, StoreToolPosInd);
            yield return Toils_Goto.GotoCell(StoreToolPosInd, PathEndMode.OnCell);
            yield return Toils_Haul.PlaceHauledThingInCell(StoreToolPosInd, null, true);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref maxTick, "maxTick");
            Scribe_Values.Look(ref needManipulate, "needManipulate");
        }
    }
}