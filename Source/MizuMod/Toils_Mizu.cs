using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public static class Toils_Mizu
    {
        public static T FailOnChangingTerrain<T>(this T f, TargetIndex index,
            List<WaterTerrainType> waterTerrainTypeList) where T : IJobEndable
        {
            f.AddEndCondition(() =>
            {
                var thing = f.GetActor().jobs.curJob.GetTarget(index).Thing;
                var terrainDef = thing.Map.terrainGrid.TerrainAt(thing.Position);
                if (!waterTerrainTypeList.Contains(terrainDef.GetWaterTerrainType()))
                {
                    return JobCondition.Incompletable;
                }

                return JobCondition.Ongoing;
            });
            return f;
        }

        public static Toil DoRecipeWorkDrawing(TargetIndex billGiverIndex)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                var jobDriver_DoBill = (JobDriver_DoBill) actor.jobs.curDriver;

                jobDriver_DoBill.workLeft = curJob.bill.recipe.WorkAmountTotal(null);
                jobDriver_DoBill.billStartTick = Find.TickManager.TicksGame;
                jobDriver_DoBill.ticksSpentDoingRecipeWork = 0;

                curJob.bill.Notify_DoBillStarted(actor);
            };
            toil.tickAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                var jobDriver_DoBill = (JobDriver_DoBill) actor.jobs.curDriver;

                jobDriver_DoBill.ticksSpentDoingRecipeWork++;
                curJob.bill.Notify_PawnDidWork(actor);

                if (actor.CurJob.GetTarget(billGiverIndex).Thing is IBillGiverWithTickAction billGiverWithTickAction)
                {
                    // 設備の時間経過処理
                    billGiverWithTickAction.UsedThisTick();
                }

                // 工数を進める処理
                var num = curJob.RecipeDef.workSpeedStat != null
                    ? actor.GetStatValue(curJob.RecipeDef.workSpeedStat)
                    : 1f;
                if (jobDriver_DoBill.BillGiver is Building_WorkTable building_WorkTable)
                {
                    num *= building_WorkTable.GetStatValue(StatDefOf.WorkTableWorkSpeedFactor);
                }

                if (DebugSettings.fastCrafting)
                {
                    num *= 30f;
                }

                jobDriver_DoBill.workLeft -= num;

                // 椅子から快適さを得る
                actor.GainComfortFromCellIfPossible();

                // 完了チェック
                if (jobDriver_DoBill.workLeft <= 0f)
                {
                    jobDriver_DoBill.ReadyForNextToil();
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.WithEffect(() => toil.actor.CurJob.bill.recipe.effectWorking, billGiverIndex);
            toil.PlaySustainerOrSound(() => toil.actor.CurJob.bill.recipe.soundWorking);
            toil.WithProgressBar(billGiverIndex, delegate
            {
                var actor = toil.actor;
                var curJob = actor.CurJob;
                return 1f - (((JobDriver_DoBill) actor.jobs.curDriver).workLeft /
                             curJob.bill.recipe.WorkAmountTotal(null));
            });
            toil.FailOn(() => toil.actor.CurJob.bill.suspended);
            return toil;
        }

        public static Toil FinishRecipeAndStartStoringProduct(Func<Thing> makeRecipeProduct)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                var jobDriver_DoBill = (JobDriver_DoBill) actor.jobs.curDriver;

                // 経験値取得
                if (curJob.RecipeDef.workSkill != null)
                {
                    var xp = jobDriver_DoBill.ticksSpentDoingRecipeWork * 0.11f * curJob.RecipeDef.workSkillLearnFactor;
                    actor.skills.GetSkill(curJob.RecipeDef.workSkill).Learn(xp);
                }

                // 生産物の生成
                var thing = makeRecipeProduct();
                if (thing == null)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                    return;
                }

                curJob.bill.Notify_IterationCompleted(actor, null);

                // 水汲み記録追加
                actor.records.AddTo(MizuDef.Record_WaterDrew, thing.stackCount);
                //RecordsUtility.Notify_BillDone(actor, new List<Thing>() { thing });

                // 床置き指定
                if (curJob.bill.GetStoreMode() == BillStoreModeDefOf.DropOnFloor)
                {
                    if (!GenPlace.TryPlaceThing(thing, actor.Position, actor.Map, ThingPlaceMode.Near))
                    {
                        Log.Error(string.Concat(actor, " could not drop recipe product ", thing, " near ",
                            actor.Position));
                    }

                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                    return;
                }

                // 最適な倉庫まで持っていく
                thing.SetPositionDirect(actor.Position);
                if (StoreUtility.TryFindBestBetterStoreCellFor(thing, actor, actor.Map, StoragePriority.Unstored,
                    actor.Faction, out var c))
                {
                    actor.carryTracker.TryStartCarry(thing);
                    curJob.targetA = thing;
                    curJob.targetB = c;
                    curJob.count = 99999;
                    return;
                }

                if (!GenPlace.TryPlaceThing(thing, actor.Position, actor.Map, ThingPlaceMode.Near))
                {
                    Log.Error(string.Concat("Bill doer could not drop product ", thing, " near ", actor.Position));
                }

                actor.jobs.EndCurrentJob(JobCondition.Succeeded);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static Toil AddPlacedThing()
        {
            var toil = new Toil();
            toil.initAction = () =>
            {
                var actor = toil.actor;
                if (actor.CurJob.placedThings == null)
                {
                    actor.CurJob.placedThings = new List<ThingCountClass>();
                }

                actor.CurJob.placedThings.Add(new ThingCountClass(actor.carryTracker.CarriedThing,
                    actor.carryTracker.CarriedThing.stackCount));
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static Toil FinishPourRecipe(TargetIndex billGiverIndex)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                var jobDriver_DoBill = (JobDriver_DoBill) actor.jobs.curDriver;

                // 経験値取得
                if (curJob.RecipeDef.workSkill != null)
                {
                    var xp = jobDriver_DoBill.ticksSpentDoingRecipeWork * 0.11f * curJob.RecipeDef.workSkillLearnFactor;
                    actor.skills.GetSkill(curJob.RecipeDef.workSkill).Learn(xp);
                }

                // 注ぎ込んだ水の総量と水質を求める
                var totalWaterVolume = 0f;
                var totalWaterType = WaterType.NoWater;
                foreach (var tspc in curJob.placedThings)
                {
                    var thingDef = tspc.thing.def;
                    var compprop = thingDef.GetCompProperties<CompProperties_WaterSource>();
                    if (compprop == null)
                    {
                        Log.Error("compprop is null");
                        actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                        return;
                    }

                    totalWaterVolume += compprop.waterVolume * tspc.Count;
                    totalWaterType = totalWaterType.GetMinType(compprop.waterType);
                }

                if (!(curJob.GetTarget(billGiverIndex).Thing is Building_WaterNetWorkTable billGiver))
                {
                    Log.Error("billGiver is null");
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                // 水の増加
                billGiver.AddWaterVolume(totalWaterVolume);
                // 水質変更
                billGiver.TankComp.StoredWaterType = billGiver.TankComp.StoredWaterType.GetMinType(totalWaterType);

                // 水アイテムの消費
                foreach (var tspc in curJob.placedThings)
                {
                    var thing = tspc.Count < tspc.thing.stackCount ? tspc.thing.SplitOff(tspc.Count) : tspc.thing;

                    curJob.RecipeDef.Worker.ConsumeIngredient(thing, curJob.RecipeDef, actor.Map);
                }

                curJob.bill.Notify_IterationCompleted(actor, null);
                //RecordsUtility.Notify_BillDone(actor, new List<Thing>());

                actor.jobs.EndCurrentJob(JobCondition.Succeeded);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static Toil StartCarryFromInventory(TargetIndex thingIndex)
        {
            // 水(食事)を持ち物から取り出す
            var toil = new Toil();
            toil.initAction = () =>
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                var thing = curJob.GetTarget(thingIndex).Thing;
                if (actor.inventory == null || thing == null)
                {
                    return;
                }

                actor.inventory.innerContainer.Take(thing);
                actor.carryTracker.TryStartCarry(thing);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.FailOnDestroyedOrNull(thingIndex);
            return toil;
        }

        public static Toil StartPathToDrinkSpot(TargetIndex thingIndex)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;

                var intVec = RCellFinder.SpotToChewStandingNear(actor, actor.CurJob.GetTarget(thingIndex).Thing);
                actor.Map.pawnDestinationReservationManager.Reserve(actor, actor.CurJob, intVec);
                actor.pather.StartPath(intVec, PathEndMode.OnCell);
            };
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }

        private static Toil DrinkSomeone(TargetIndex thingIndex, Func<Toil, Func<LocalTargetInfo>> funcGetter)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var thing = actor.CurJob.GetTarget(thingIndex).Thing;
                var comp = thing.TryGetComp<CompWaterSource>();
                if (comp == null || comp.SourceType != CompProperties_WaterSource.SourceType.Item)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                actor.rotationTracker.FaceCell(actor.Position);
                if (!thing.CanDrinkWaterNow())
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                actor.jobs.curDriver.ticksLeftThisToil = comp.BaseDrinkTicks;
                if (thing.Spawned)
                {
                    thing.Map.physicalInteractionReservationManager.Reserve(actor, actor.CurJob, thing);
                }
            };
            toil.tickAction = delegate { toil.actor.GainComfortFromCellIfPossible(); };
            toil.WithProgressBar(thingIndex, delegate
            {
                var actor = toil.actor;
                var thing = actor.CurJob.GetTarget(thingIndex).Thing;
                var comp = thing.TryGetComp<CompWaterSource>();
                if (thing == null || comp == null || comp.SourceType != CompProperties_WaterSource.SourceType.Item)
                {
                    return 1f;
                }

                return 1f - (toil.actor.jobs.curDriver.ticksLeftThisToil / (float) comp.BaseDrinkTicks);
            });
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnDestroyedOrNull(thingIndex);
            toil.AddFinishAction(delegate
            {
                var actor = toil.actor;

                var thing = actor?.CurJob?.GetTarget(thingIndex).Thing;
                if (thing == null)
                {
                    return;
                }

                if (actor.Map.physicalInteractionReservationManager.IsReservedBy(actor, thing))
                {
                    actor.Map.physicalInteractionReservationManager.Release(actor, actor.CurJob, thing);
                }
            });

            // エフェクト追加
            toil.WithEffect(delegate
            {
                var target = toil.actor.CurJob.GetTarget(thingIndex);
                if (!target.HasThing)
                {
                    return null;
                }

                EffecterDef effecter = null;
                var comp = target.Thing.TryGetComp<CompWaterSource>();
                if (comp != null)
                {
                    effecter = comp.GetEffect;
                }

                return effecter;
            }, funcGetter(toil));
            toil.PlaySustainerOrSound(delegate
            {
                var actor = toil.actor;
                if (!actor.RaceProps.Humanlike)
                {
                    return null;
                }

                var target = toil.actor.CurJob.GetTarget(thingIndex);
                if (!target.HasThing)
                {
                    return null;
                }

                var comp = target.Thing.TryGetComp<CompWaterSource>();

                return comp?.Props.getSound;
            });
            return toil;
        }

        public static Toil Drink(TargetIndex thingIndex)
        {
            return DrinkSomeone(thingIndex, toil => () =>
            {
                if (!toil.actor.CurJob.GetTarget(thingIndex).HasThing)
                {
                    return null;
                }

                return toil.actor.CurJob.GetTarget(thingIndex).Thing;
            });
        }

        public static Toil FeedToPatient(TargetIndex thingIndex, TargetIndex patientIndex)
        {
            return DrinkSomeone(thingIndex, toil => () =>
            {
                if (!toil.actor.CurJob.GetTarget(patientIndex).HasThing)
                {
                    return null;
                }

                if (!(toil.actor.CurJob.GetTarget(patientIndex).Thing is Pawn patient))
                {
                    return null;
                }

                return patient;
            });
        }

        private static Toil FinishDrinkSomeone(TargetIndex thingIndex, Func<Toil, Pawn> pawnGetter)
        {
            var toil = new Toil();
            toil.initAction = () =>
            {
                var thing = toil.actor.jobs.curJob.GetTarget(thingIndex).Thing;
                var getter = pawnGetter(toil);
                if (getter == null)
                {
                    return;
                }

                var wantedWaterAmount = getter.needs.Water().WaterWanted;
                var gotWaterAmount = MizuUtility.GetWater(getter, thing, wantedWaterAmount, false);
                if (!getter.Dead)
                {
                    getter.needs.Water().CurLevel += gotWaterAmount;
                }

                getter.records.AddTo(MizuDef.Record_WaterDrank, gotWaterAmount);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static Toil FinishDrink(TargetIndex thingIndex)
        {
            return FinishDrinkSomeone(thingIndex, toil => toil.actor);
        }

        public static Toil FinishDrinkPatient(TargetIndex thingIndex, TargetIndex patientIndex)
        {
            return FinishDrinkSomeone(thingIndex, toil =>
            {
                if (!toil.actor.CurJob.GetTarget(patientIndex).HasThing)
                {
                    return null;
                }

                return toil.actor.CurJob.GetTarget(patientIndex).Thing as Pawn;
            });
        }

        public static Toil DrinkTerrain(TargetIndex cellIndex, int baseDrinkTicksFromTerrain)
        {
            // 地形から水を飲む
            var initialTicks = 1;

            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var cell = actor.CurJob.GetTarget(cellIndex).Cell;
                var need_water = actor.needs.Water();
                if (need_water == null)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                var waterType = cell.GetTerrain(actor.Map).ToWaterType();
                if (waterType == WaterType.NoWater || waterType == WaterType.Undefined)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                var waterTypeDef = MizuDef.Dic_WaterTypeDef[waterType];

                // 向き変更
                actor.rotationTracker.FaceCell(actor.Position);

                // 作業量
                actor.jobs.curDriver.ticksLeftThisToil = (int) (baseDrinkTicksFromTerrain * need_water.WaterWanted);
                initialTicks = actor.jobs.curDriver.ticksLeftThisToil;

                if (actor.needs.mood != null)
                {
                    // 水分摂取による心情変化
                    var thoughtList = new List<ThoughtDef>();
                    MizuUtility.ThoughtsFromWaterTypeDef(actor, waterTypeDef, true, thoughtList);
                    foreach (var thoughtDef in thoughtList)
                    {
                        actor.needs.mood.thoughts.memories.TryGainMemory(thoughtDef);
                    }
                }

                // 指定された健康状態になる
                if (waterTypeDef.hediffs != null)
                {
                    foreach (var hediff in waterTypeDef.hediffs)
                    {
                        actor.health.AddHediff(HediffMaker.MakeHediff(hediff, actor));
                    }
                }

                // 確率で食中毒
                if (!(Rand.Value < waterTypeDef.foodPoisonChance))
                {
                    return;
                }

                actor.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.FoodPoisoning, actor));
                if (!PawnUtility.ShouldSendNotificationAbout(actor))
                {
                    return;
                }

                var water = ThingMaker.MakeThing(MizuUtility.GetWaterThingDefFromWaterType(waterType));
                string cause = "MizuPoisonedByDirtyWater".Translate().CapitalizeFirst();
                string text = "MessageFoodPoisoning".Translate(actor.LabelShort, water.ToString(), cause,
                    actor.Named("PAWN"), water.Named("FOOD")).CapitalizeFirst();
                Messages.Message(text, actor, MessageTypeDefOf.NegativeEvent);
            };
            toil.tickAction = delegate
            {
                toil.actor.GainComfortFromCellIfPossible();
                var need_water = toil.actor.needs.Water();
                //var cell = toil.actor.CurJob.GetTarget(cellIndex).Cell;
                if (need_water == null)
                {
                    toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                // 徐々に飲む
                var riseNeedWater = 1 / (float) baseDrinkTicksFromTerrain;
                need_water.CurLevel = Mathf.Min(need_water.CurLevel + riseNeedWater, need_water.MaxLevel);
            };
            toil.WithProgressBar(cellIndex,
                () => 1f - ((float) toil.actor.jobs.curDriver.ticksLeftThisToil / initialTicks));
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOn(t =>
            {
                var actor = toil.actor;
                return actor.CurJob.targetA.Cell.IsForbidden(actor) ||
                       !actor.CanReach(actor.CurJob.targetA.Cell, PathEndMode.OnCell, Danger.Deadly);
            });

            // エフェクト追加
            toil.PlaySustainerOrSound(() => DefDatabase<SoundDef>.GetNamed("Ingest_Water"));
            return toil;
        }

        public static Toil FinishDrinkTerrain(TargetIndex terrainVecIndex)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var need_water = actor.needs.Water();

                var numWater = need_water.MaxLevel - need_water.CurLevel;

                var terrain = actor.Map.terrainGrid.TerrainAt(actor.CurJob.GetTarget(terrainVecIndex).Cell);
                var drankTerrainType = terrain.GetWaterTerrainType();

                if (actor.needs.mood != null)
                {
                    // 直接飲んだ
                    actor.needs.mood.thoughts.memories.TryGainMemory(actor.CanManipulate()
                        ? MizuDef.Thought_DrankScoopedWater
                        : MizuDef.Thought_SippedWaterLikeBeast);

                    var thoughtDef = MizuUtility.GetThoughtDefFromTerrainType(drankTerrainType);
                    if (thoughtDef != null)
                    {
                        // 水の種類による心情
                        actor.needs.mood.thoughts.memories.TryGainMemory(thoughtDef);
                    }
                }

                if (drankTerrainType == WaterTerrainType.SeaWater)
                {
                    // 海水の場合の健康状態悪化
                    actor.health.AddHediff(HediffMaker.MakeHediff(MizuDef.Hediff_DrankSeaWater, actor));
                }

                if (!actor.Dead)
                {
                    actor.needs.Water().CurLevel += numWater;
                }

                actor.records.AddTo(MizuDef.Record_WaterDrank, numWater);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static Toil DropCarriedThing(TargetIndex prisonerIndex, TargetIndex dropSpotIndex)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;

                // そもそも何も運んでいない
                if (actor.carryTracker?.CarriedThing == null)
                {
                    return;
                }

                // ターゲットが場所ではなく物
                if (actor.CurJob.GetTarget(dropSpotIndex).HasThing)
                {
                    return;
                }


                // その場に置いてみる
                var isDropSuccess = actor.carryTracker.TryDropCarriedThing(actor.CurJob.GetTarget(dropSpotIndex).Cell,
                    ThingPlaceMode.Direct, out var dropThing);

                if (!isDropSuccess)
                {
                    // その場に置けなかったら近くに置いてみる
                    isDropSuccess = actor.carryTracker.TryDropCarriedThing(actor.CurJob.GetTarget(dropSpotIndex).Cell,
                        ThingPlaceMode.Near, out dropThing);
                }

                // その場or近くに置けなかった
                if (!isDropSuccess)
                {
                    return;
                }

                if (actor.Map.reservationManager.ReservedBy(dropThing, actor))
                {
                    // 持ってる人に予約されているなら、解放する
                    actor.Map.reservationManager.Release(dropThing, actor, actor.CurJob);
                }

                // 相手が囚人でない可能性
                if (!actor.CurJob.GetTarget(prisonerIndex).HasThing)
                {
                    return;
                }


                // 囚人がポーンではない
                if (!(actor.CurJob.GetTarget(prisonerIndex).Thing is Pawn))
                {
                }

                //// 置いた水を囚人が飲むジョブを作成
                //Job job = new Job(MizuDef.Job_DrinkWater, dropThing)
                //{
                //    count = MizuUtility.WillGetStackCountOf(prisoner, dropThing)
                //};
                //// ジョブを囚人の待ち行列に加える
                //prisoner.jobs.jobQueue.EnqueueLast(job, JobTag.SatisfyingNeeds);
                //// 新ジョブに対して水を予約させる
                //prisoner.Map.reservationManager.Reserve(prisoner, job, dropThing);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            toil.atomicWithPrevious = true;
            return toil;
        }

        public static Toil AddCarriedThingToInventory()
        {
            var toil = new Toil();
            toil.initAction = () =>
            {
                var actor = toil.actor;
                if (actor.carryTracker?.innerContainer == null)
                {
                    return;
                }

                if (actor.inventory?.innerContainer == null)
                {
                    return;
                }

                var takeThing = actor.carryTracker.innerContainer.Take(actor.carryTracker.CarriedThing);
                actor.inventory.innerContainer.TryAdd(takeThing);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;

            return toil;
        }

        public static Toil DrinkFromBuilding(TargetIndex buildingIndex)
        {
            var initialTicks = 1;

            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var thing = actor.CurJob.GetTarget(buildingIndex).Thing;
                var comp = thing.TryGetComp<CompWaterSource>();

                if (actor.needs?.Water() == null || !(thing is IBuilding_DrinkWater) || comp == null ||
                    !comp.IsWaterSource)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                var need_water = actor.needs.Water();
                var waterTypeDef = MizuDef.Dic_WaterTypeDef[comp.WaterType];

                // 向きを変更
                actor.rotationTracker.FaceCell(actor.Position);

                // 作業量
                actor.jobs.curDriver.ticksLeftThisToil = (int) (comp.BaseDrinkTicks * need_water.WaterWanted);
                initialTicks = actor.jobs.curDriver.ticksLeftThisToil;

                if (actor.needs.mood != null)
                {
                    // 水分摂取による心情変化
                    foreach (var thoughtDef in MizuUtility.ThoughtsFromGettingWater(actor, thing))
                    {
                        actor.needs.mood.thoughts.memories.TryGainMemory(thoughtDef);
                    }
                }

                // 指定された健康状態になる
                if (waterTypeDef.hediffs != null)
                {
                    foreach (var hediff in waterTypeDef.hediffs)
                    {
                        actor.health.AddHediff(HediffMaker.MakeHediff(hediff, actor));
                    }
                }

                // 確率で食中毒
                if (Rand.Value < waterTypeDef.foodPoisonChance)
                {
                    FoodUtility.AddFoodPoisoningHediff(actor, thing, FoodPoisonCause.Unknown);
                }
            };
            toil.tickAction = delegate
            {
                toil.actor.GainComfortFromCellIfPossible();
                var need_water = toil.actor.needs.Water();
                var thing = toil.actor.CurJob.GetTarget(buildingIndex).Thing;
                var comp = thing.TryGetComp<CompWaterSource>();
                if (thing == null || comp == null || !comp.IsWaterSource || !(thing is IBuilding_DrinkWater building) ||
                    building.IsEmpty)
                {
                    toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                // 徐々に飲む
                var riseNeedWater = 1 / (float) comp.BaseDrinkTicks;
                need_water.CurLevel = Mathf.Min(need_water.CurLevel + riseNeedWater, need_water.MaxLevel);
                building.DrawWater(riseNeedWater * Need_Water.NeedWaterVolumePerDay);
            };
            toil.WithProgressBar(buildingIndex,
                () => 1f - ((float) toil.actor.jobs.curDriver.ticksLeftThisToil / initialTicks));
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOn(t =>
            {
                var actor = toil.actor;
                var target = actor.CurJob.GetTarget(buildingIndex);

                if (target.Thing.def.hasInteractionCell)
                {
                    // 使用場所があるなら使用場所基準
                    return target.Thing.InteractionCell.IsForbidden(actor) ||
                           !actor.CanReach(target.Thing.InteractionCell, PathEndMode.OnCell, Danger.Deadly);
                }

                // 使用場所がないなら設備の場所基準
                return target.Thing.Position.IsForbidden(actor) ||
                       !actor.CanReach(target.Thing.Position, PathEndMode.ClosestTouch, Danger.Deadly);
            });

            // エフェクト追加
            toil.PlaySustainerOrSound(() => DefDatabase<SoundDef>.GetNamed("Ingest_Water"));
            return toil;
        }

        public static Toil DrawWater(TargetIndex drawerIndex, int drawTicks)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var thing = actor.CurJob.GetTarget(drawerIndex).Thing;
                if (!(thing is IBuilding_DrinkWater building)
                    || building.WaterType == WaterType.Undefined
                    || building.WaterType == WaterType.NoWater
                    || !building.CanDrawFor(actor))
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                actor.rotationTracker.FaceCell(actor.Position);
                actor.jobs.curDriver.ticksLeftThisToil = drawTicks;
            };
            toil.tickAction = delegate { toil.actor.GainComfortFromCellIfPossible(); };
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.FailOnDestroyedOrNull(drawerIndex);

            // エフェクト追加
            toil.WithEffect(DefDatabase<EffecterDef>.GetNamed("Cook"), drawerIndex);
            toil.PlaySustainerOrSound(DefDatabase<SoundDef>.GetNamed("Pour_Water"));
            return toil;
        }

        public static Toil FinishDrawWater(TargetIndex drawerIndex)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                var thing = curJob.GetTarget(drawerIndex).Thing;
                if (thing == null)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                if (!(thing is IBuilding_DrinkWater building))
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                // 生産物の生成
                // 地下水脈の水の種類から水アイテムの種類を決定
                var waterThingDef = MizuUtility.GetWaterThingDefFromWaterType(building.WaterType);
                if (waterThingDef == null)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                // 水アイテムの水源情報を得る
                var compprop = waterThingDef.GetCompProperties<CompProperties_WaterSource>();
                if (compprop == null)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                // 地下水脈から水を減らす
                building.DrawWater(compprop.waterVolume);

                // 水を生成
                var createThing = ThingMaker.MakeThing(waterThingDef);
                if (createThing == null)
                {
                    actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                    return;
                }

                // 個数設定
                createThing.stackCount = 1;

                // 水汲み記録追加
                actor.records.AddTo(MizuDef.Record_WaterDrew, 1);

                // 床置き指定
                if (!GenPlace.TryPlaceThing(createThing, actor.Position, actor.Map, ThingPlaceMode.Near))
                {
                    Log.Error(string.Concat(actor, " could not drop recipe product ", thing, " near ", actor.Position));
                }

                actor.jobs.EndCurrentJob(JobCondition.Succeeded);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }

        public static Toil UpdateResourceCounts()
        {
            var toil = new Toil();
            toil.initAction = () =>
            {
                if (toil.actor.jobs.curJob.bill is Bill_Production billProduction &&
                    billProduction.repeatMode == BillRepeatModeDefOf.TargetCount)
                {
                    toil.actor.Map.resourceCounter.UpdateResourceCounts();
                }
            };

            return toil;
        }

        public static Toil ClearConditionSatisfiedTargets(TargetIndex ind, Predicate<LocalTargetInfo> cond)
        {
            var toil = new Toil();
            toil.initAction = delegate
            {
                var actor = toil.actor;
                var curJob = actor.jobs.curJob;
                var targetQueue = curJob.GetTargetQueue(ind);
                targetQueue.RemoveAll(cond);
            };
            return toil;
        }

        public static Toil TryFindStoreCell(TargetIndex thingInd, TargetIndex storeInd)
        {
            var toil = new Toil();
            toil.initAction = () =>
            {
                var actor = toil.actor;
                var thing = actor.jobs.curJob.GetTarget(thingInd).Thing;

                if (!StoreUtility.TryFindBestBetterStoreCellFor(thing, actor, actor.Map, StoragePriority.Unstored,
                    actor.Faction, out var storeCell))
                {
                    // 最適な倉庫が見つからなかった→そこで終了
                    actor.jobs.EndCurrentJob(JobCondition.Succeeded);
                    return;
                }

                // 見つけた場所をセット
                actor.jobs.curJob.SetTarget(storeInd, storeCell);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            return toil;
        }
    }
}