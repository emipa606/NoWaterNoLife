using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
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
            Map map = MapWithLowWater();
            if (map == null)
            {
                return string.Empty;
            }
            var totalWater = map.resourceCounter.TotalWater();
            var num = map.mapPawns.FreeColonistsSpawnedCount + map.mapPawns.PrisonersOfColonyCount;
            var num2 = Mathf.FloorToInt(totalWater / (float)num);
            return string.Format(MizuStrings.AlertLowWaterDesc.Translate(), totalWater.ToString("F0"), num.ToStringCached(), num2.ToStringCached());
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
            List<Map> maps = Find.Maps;
            for (var i = 0; i < maps.Count; i++)
            {
                Map map = maps[i];
                if (map.IsPlayerHome)
                {
                    var freeColonistsSpawnedCount = map.mapPawns.FreeColonistsSpawnedCount;
                    if (map.resourceCounter.TotalWater() < WaterAmountThresholdPerColonist * (float)freeColonistsSpawnedCount)
                    {
                        return map;
                    }
                }
            }
            return null;
        }
    }
}
