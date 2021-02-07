using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using RimWorld;

namespace MizuMod
{
    public class CompWaterTool : ThingComp
    {
        public CompProperties_WaterTool Props => (CompProperties_WaterTool)props;

        public List<CompProperties_WaterTool.UseWorkType> UseWorkType => Props.useWorkType;
        public List<WorkTypeDef> SupplyWorkType => Props.supplyWorkType;
        public float MaxWaterVolume => Props.maxWaterVolume;

        private float storedWaterVolume;
        public float StoredWaterVolume
        {
            get => storedWaterVolume;
            set
            {
                storedWaterVolume = Mathf.Min(MaxWaterVolume, Mathf.Max(0f, value));
                if (storedWaterVolume <= 0f)
                {
                    StoredWaterType = WaterType.NoWater;
                }
            }
        }

        private WaterType storedWaterType = WaterType.NoWater;
        public WaterType StoredWaterType
        {
            get => storedWaterType;
            set => storedWaterType = value;
        }

        public float StoredWaterVolumePercent => StoredWaterVolume / MaxWaterVolume;

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref storedWaterVolume, "storedWaterVolume");
            Scribe_Values.Look(ref storedWaterType, "storedWaterType", WaterType.NoWater);
        }

        public override string CompInspectStringExtra()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.CompInspectStringExtra());

            if (stringBuilder.ToString() != string.Empty)
            {
                stringBuilder.AppendLine();
            }
            stringBuilder.Append(string.Concat(new string[]
            {
                MizuStrings.InspectWaterToolStored.Translate(),
                ":",
                (StoredWaterVolumePercent * 100).ToString("F0"),
                "%",
            }));

            if (DebugSettings.godMode)
            {
                stringBuilder.Append(string.Concat(new string[]
                {
                    "(",
                    StoredWaterVolume.ToString("F2"),
                    "/",
                    MaxWaterVolume.ToString("F2"),
                    ")",
                }));
            }

            stringBuilder.Append(string.Concat(new string[]
            {
                "(",
                MizuStrings.GetInspectWaterTypeString(StoredWaterType),
                ")",
            }));

            return stringBuilder.ToString();
        }
    }
}
