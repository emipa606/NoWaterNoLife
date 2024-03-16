using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace MizuMod;

public class CompWaterTool : ThingComp
{
    private WaterType storedWaterType = WaterType.NoWater;

    private float storedWaterVolume;

    public float MaxWaterVolume => Props.maxWaterVolume;

    public CompProperties_WaterTool Props => (CompProperties_WaterTool)props;

    public WaterType StoredWaterType
    {
        get => storedWaterType;
        set => storedWaterType = value;
    }

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

    public float StoredWaterVolumePercent => StoredWaterVolume / MaxWaterVolume;

    public List<WorkTypeDef> SupplyWorkType => Props.supplyWorkType;

    public List<CompProperties_WaterTool.UseWorkType> UseWorkType => Props.useWorkType;

    public override string CompInspectStringExtra()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.CompInspectStringExtra());

        if (stringBuilder.ToString() != string.Empty)
        {
            stringBuilder.AppendLine();
        }

        stringBuilder.Append(
            string.Concat(
            [
                MizuStrings.InspectWaterToolStored.Translate().RawText, ":",
                (StoredWaterVolumePercent * 100).ToString("F0"), "%"
            ]));

        if (DebugSettings.godMode)
        {
            stringBuilder.Append(
                $"({StoredWaterVolume:F2}/{MaxWaterVolume:F2})");
        }

        stringBuilder.Append(
            string.Concat(["(", MizuStrings.GetInspectWaterTypeString(StoredWaterType), ")"]));

        return stringBuilder.ToString();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Values.Look(ref storedWaterVolume, "storedWaterVolume");
        Scribe_Values.Look(ref storedWaterType, "storedWaterType", WaterType.NoWater);
    }
}