using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_SupplyWaterToTool : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest =>
            ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            // 水設備チェック
            var compSource = t.TryGetComp<CompWaterSource>();
            if (compSource == null)
            {
                return null;
            }

            // 許可不許可チェック
            if (t.IsForbidden(pawn))
            {
                return null;
            }

            // 派閥チェック
            if (t.Faction != pawn.Faction)
            {
                return null;
            }

            // 予約チェック
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return null;
            }

            // 設備でなければだめ
            if (compSource.SourceType != CompProperties_WaterSource.SourceType.Building)
            {
                return null;
            }

            if (!(t is IBuilding_DrinkWater sourceBuilding))
            {
                return null;
            }

            // 水が汲めるかチェック(仮)
            if (!sourceBuilding.CanDrawFor(pawn))
            {
                return null;
            }

            // 水ツールチェック
            var toolList = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways)
                .FindAll(thing => thing.TryGetComp<CompWaterTool>() != null);

            // 一番近いツールを使う
            Thing minTool = null;
            var minDist = float.MaxValue;
            foreach (var tool in toolList)
            {
                var compTool = tool.TryGetComp<CompWaterTool>();
                if (compTool == null)
                {
                    continue;
                }

                // ワークタイプがその水ツールに設定された水補充ワークタイプの中に含まれているか
                if (!compTool.SupplyWorkType.Contains(def.workType))
                {
                    continue;
                }

                // 表示が100%の場合は汲まない
                if (compTool.StoredWaterVolumePercent >= 0.995f)
                {
                    continue;
                }

                // 設備の水が足りない
                if (compTool.MaxWaterVolume - compTool.StoredWaterVolume > sourceBuilding.WaterVolume)
                {
                    continue;
                }

                // 許可不許可チェック
                if (tool.IsForbidden(pawn))
                {
                    continue;
                }

                // 予約チェック
                if (!pawn.CanReserve(tool, 1, -1, null, forced))
                {
                    continue;
                }

                // 距離チェック
                float dist = (tool.Position - pawn.Position).LengthHorizontalSquared;
                if (!(minDist > dist))
                {
                    continue;
                }

                minDist = dist;
                minTool = tool;
            }

            if (minTool == null)
            {
                return null;
            }

            var job = new Job(MizuDef.Job_SupplyWaterToTool)
            {
                targetA = t,
                targetB = minTool,
                count = 1
            };

            return job;
        }
    }
}