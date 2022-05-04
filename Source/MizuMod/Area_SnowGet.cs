using UnityEngine;
using Verse;

namespace MizuMod;

public class Area_SnowGet : Area
{
    public Area_SnowGet()
    {
    }

    public Area_SnowGet(AreaManager areaManager) : base(areaManager)
    {
    }

    public override string Label => MizuStrings.AreaSnowGet.Translate();

    public override Color Color => new Color(0.5f, 0.0f, 0.0f);

    public override int ListPriority => 4000;

    public override string GetUniqueLoadID()
    {
        return $"Area_{ID}_MizuSnowGet";
    }
}