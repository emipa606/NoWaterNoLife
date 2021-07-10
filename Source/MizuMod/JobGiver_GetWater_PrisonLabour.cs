/*
 * Created by SharpDevelop.
 * User: Michael
 * Date: 11/4/2018
 * Time: 5:05 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using Verse;
using Verse.AI;

namespace MizuMod
{
    /// <summary>
    ///     Description of JobGiver_GetWater_PrisonLabor.
    /// </summary>
    public class JobGiver_GetWater_PrisonLabour : ThinkNode_JobGiver
    {
        // private const int MaxDistanceOfSearchWaterTerrain = 300;
        private const int SearchWaterIntervalTick = 180;

        private ThirstCategory minCategory = ThirstCategory.SlightlyThirsty;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            if (!(base.DeepCopy(resolve) is JobGiver_GetWater_PrisonLabour jobGiver_GetWater_PrisonLabour))
            {
                return null;
            }

            jobGiver_GetWater_PrisonLabour.minCategory = minCategory;
            return jobGiver_GetWater_PrisonLabour;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            var need_water = pawn.needs.Water();
            if (need_water == null)
            {
                return null;
            }

            if (need_water.lastSearchWaterTick + SearchWaterIntervalTick > Find.TickManager.TicksGame)
            {
                return null;
            }

            // Only drink if we're really thirsty.  
            if (need_water.CurLevelPercentage > need_water.PercentageThreshThirsty)
            {
                return null;
            }

            need_water.lastSearchWaterTick = Find.TickManager.TicksGame;

            var thing = MizuUtility.TryFindBestWaterSourceFor(pawn, pawn, false);
            if (thing != null)
            {
                if (thing.CanDrinkWater())
                {
                    return new Job(MizuDef.Job_DrinkWater, thing)
                    {
                        count = MizuUtility.WillGetStackCountOf(pawn, thing)
                    };
                }

                if (thing is IBuilding_DrinkWater)
                {
                    return new Job(MizuDef.Job_DrinkWaterFromBuilding, thing);
                }
            }

            // 何も見つからなかった場合は隠し水飲み場を探す
            // 人間、家畜、野生の動物全て
            if (MizuUtility.TryFindHiddenWaterSpot(pawn, out var hiddenWaterSpot))
            {
                return new Job(MizuDef.Job_DrinkWater, hiddenWaterSpot) {count = 1};
            }

            // 水を発見できず
            return null;
        }
    }
}