using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class CompWaterSource : ThingComp
    {
        public CompProperties_WaterSource Props => (CompProperties_WaterSource)props;

        public CompProperties_WaterSource.SourceType SourceType => Props.sourceType;
        public EffecterDef GetEffect => Props.getEffect;
        public SoundDef GetSound => Props.getSound;
        public int BaseDrinkTicks => Props.baseDrinkTicks;
        public bool NeedManipulate => Props.needManipulate;
        public float WaterAmount => Props.waterAmount;
        public int MaxNumToGetAtOnce => Props.maxNumToGetAtOnce;
        public float WaterVolume => Props.waterVolume;

        public WaterType WaterType
        {
            get
            {
                switch (SourceType)
                {
                    case CompProperties_WaterSource.SourceType.Item:
                        return Props.waterType;
                    case CompProperties_WaterSource.SourceType.Building:
                        if (!(parent is IBuilding_DrinkWater building))
                        {
                            return WaterType.Undefined;
                        }

                        return building.WaterType;
                    default:
                        return WaterType.Undefined;
                }
            }
        }

        public bool DependIngredients => Props.dependIngredients;

        public bool IsWaterSource => WaterType != WaterType.Undefined && WaterType != WaterType.NoWater;

        public float DrainWaterFlow => Props.drainWaterFlow;

        private CompFlickable compFlickable = null;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            compFlickable = parent.GetComp<CompFlickable>();
        }
        //public override void PostIngested(Pawn ingester)
        //{
        //    base.PostIngested(ingester);

        //    Need_Water need_water = ingester.needs.water();
        //    if (need_water == null) return;

        //    float gotWaterAmount = MizuUtility.GetWater(ingester, this.parent, need_water.WaterWanted, true);
        //    if (!ingester.Dead)
        //    {
        //        need_water.CurLevel += gotWaterAmount;
        //    }
        //    ingester.records.AddTo(MizuDef.Record_WaterDrank, gotWaterAmount);
        //}

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            foreach (var floatMenuOption in base.CompFloatMenuOptions(selPawn))
            {
                yield return floatMenuOption;
            }

            if (SourceType == CompProperties_WaterSource.SourceType.Item && !parent.def.IsIngestible)
            {
                // 水アイテムで、食べることが出来ないものは飲める

                if (selPawn.IsColonistPlayerControlled)
                {
                    var stringBuilder = new StringBuilder();
                    stringBuilder.Append(string.Format(MizuStrings.FloatMenuGetWater.Translate(), parent.LabelNoCount).CapitalizeFirst());

                    if (!parent.IsSociallyProper(selPawn))
                    {
                        // 囚人部屋のものは表示を追加
                        stringBuilder.Append(string.Concat(
                            " (",
                            "ReservedForPrisoners".Translate(),
                            ")"
                        ));
                    }
                    foreach (var p in parent.Map.mapPawns.AllPawns)
                    {
                        if (parent.Map.reservationManager.ReservedBy(parent, p))
                        {
                            // 予約されている物は表示を追加
                            stringBuilder.AppendLine();
                            stringBuilder.Append(string.Format(string.Concat(
                                " (",
                                "ReservedBy".Translate(p.LabelShort, p),
                                ")")));

                            break;
                        }
                    }

                    yield return new FloatMenuOption(stringBuilder.ToString(), () =>
                    {
                        var job = new Job(MizuDef.Job_DrinkWater, parent)
                        {
                            count = MizuUtility.WillGetStackCountOf(selPawn, parent)
                        };
                        selPawn.jobs.TryTakeOrderedJob(job, JobTag.SatisfyingNeeds);
                    });
                }
            }
        }
    }
}
