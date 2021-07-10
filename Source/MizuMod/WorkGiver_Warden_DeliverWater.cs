using RimWorld;
using Verse;
using Verse.AI;

namespace MizuMod
{
    public class WorkGiver_Warden_DeliverWater : WorkGiver_Warden
    {
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var warden = pawn;
            var prisoner = t as Pawn;

            // 世話が必要でない
            if (!ShouldTakeCareOfPrisoner(warden, prisoner))
            {
                return null;
            }

            // 囚人が食事を持って来てもらえる扱いではない
            if (prisoner != null && !prisoner.guest.CanBeBroughtFood)
            {
                return null;
            }

            // 囚人は牢屋にいない
            if (prisoner != null && !prisoner.Position.IsInPrisonCell(prisoner.Map))
            {
                return null;
            }

            if (prisoner != null)
            {
                var need_water = prisoner.needs.Water();

                // 水分要求がない
                if (need_water == null)
                {
                    return null;
                }

                // 喉が渇いていない
                if (need_water.CurLevelPercentage >= need_water.PercentageThreshThirsty + 0.02f)
                {
                    return null;
                }
            }

            // (囚人が病人だから)食事を与えられるべき状態である(部屋に運ばれたものを自分で食べることができない)
            if (WardenFeedUtility.ShouldBeFed(prisoner))
            {
                return null;
            }

            // 水が見つからない
            var thing = MizuUtility.TryFindBestWaterSourceFor(warden, prisoner, false);
            if (thing == null)
            {
                return null;
            }

            // 見つかった水アイテムは既に囚人がいる部屋の中にある
            if (thing.GetRoom() == prisoner.GetRoom())
            {
                return null;
            }

            // 部屋の中に十分な量の水がある
            if (WaterAvailableInRoomTo(prisoner))
            {
                return null;
            }

            // 水を運んでくるジョブを発行
            return new Job(MizuDef.Job_DeliverWater, thing, prisoner)
            {
                count = MizuUtility.WillGetStackCountOf(prisoner, thing),
                targetC = RCellFinder.SpotToChewStandingNear(prisoner, thing)
            };
        }

        private static float WaterAmountAvailableForFrom(Thing waterSource)
        {
            // その物は水分を得られるものではない
            if (!waterSource.CanGetWater())
            {
                return 0.0f;
            }

            return waterSource.GetWaterAmount() * waterSource.stackCount;
        }

        private static bool WaterAvailableInRoomTo(Pawn prisoner)
        {
            // 囚人が何か物を運んでいる＆その物から得られる水分量は正の値
            if (prisoner.carryTracker.CarriedThing != null
                && WaterAmountAvailableForFrom(prisoner.carryTracker.CarriedThing) > 0f)
            {
                return true;
            }

            var allPawnWantedWater = 0.0f;
            var allThingWaterAmount = 0f;

            var room = prisoner.GetRoom();
            if (room == null)
            {
                return false;
            }

            foreach (var region in room.Regions)
            {
                // 囚人の部屋の中の全水アイテムの水分量を計算
                foreach (var thing in region.ListerThings.ThingsInGroup(ThingRequestGroup.HaulableEver))
                {
                    if (!thing.IsIngestibleFor(prisoner)
                        && (!thing.CanDrinkWater() || thing.GetWaterPreferability() > WaterPreferability.NeverDrink))
                    {
                        allThingWaterAmount += WaterAmountAvailableForFrom(thing);
                    }
                }

                // 囚人の部屋のポーンの要求水分量の合計を計算
                foreach (var thing in region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn))
                {
                    if (!(thing is Pawn pawn))
                    {
                        continue;
                    }

                    var need_water = pawn.needs.Water();

                    // 水要求なし
                    if (need_water == null)
                    {
                        continue;
                    }

                    // コロニーの囚人ではない
                    if (!pawn.IsPrisonerOfColony)
                    {
                        continue;
                    }

                    // 喉が渇いていない
                    if (need_water.CurLevelPercentage >= need_water.PercentageThreshThirsty + 0.02f)
                    {
                        continue;
                    }

                    // 物を運んでいる
                    if (pawn.carryTracker.CarriedThing != null)
                    {
                        continue;
                    }

                    allPawnWantedWater += need_water.WaterWanted;
                }
            }

            // その部屋に十分な水の量があればtrue
            return allThingWaterAmount + 0.5f >= allPawnWantedWater;
        }
    }
}