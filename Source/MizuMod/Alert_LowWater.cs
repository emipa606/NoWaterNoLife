using RimWorld;
using UnityEngine;
using Verse;

namespace MizuMod
{
    public class Alert_LowWater : Alert
    {
        private const float WaterAmountThresholdPerColonist = 4f;

        public Alert_LowWater()
        {
            defaultLabel = MizuStrings.AlertLowWater.Translate();
            defaultPriority = AlertPriority.High;
        }

        public override TaggedString GetExplanation()
        {
            var map = MapWithLowWater();
            if (map == null)
            {
                return string.Empty;
            }

            var totalWater = map.resourceCounter.TotalWater();
            var num = map.mapPawns.FreeColonistsSpawnedCount + map.mapPawns.PrisonersOfColonyCount;
            var num2 = Mathf.FloorToInt(totalWater / num);
            return string.Format(
                MizuStrings.AlertLowWaterDesc.Translate(),
                totalWater.ToString("F0"),
                num.ToStringCached(),
                num2.ToStringCached());
        }

        public override AlertReport GetReport()
        {
            if (Find.TickManager.TicksGame < 150000)
            {
                return false;
            }

            return MapWithLowWater() != null;
        }

        private Map MapWithLowWater()
        {
            var maps = Find.Maps;
            foreach (var map in maps)
            {
                if (!map.IsPlayerHome)
                {
                    continue;
                }

                var freeColonistsSpawnedCount = map.mapPawns.FreeColonistsSpawnedCount;
                if (map.resourceCounter.TotalWater() < WaterAmountThresholdPerColonist * freeColonistsSpawnedCount)
                {
                    return map;
                }
            }

            return null;
        }
    }
}