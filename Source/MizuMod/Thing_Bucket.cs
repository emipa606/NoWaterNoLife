using System.Collections.Generic;
using Verse;

namespace MizuMod;

public class Thing_Bucket : ThingWithComps
{
    private readonly List<float> graphicThreshold =
    [
        0.9f,
        100f
    ];

    private CompWaterTool compWaterTool;

    private int graphicIndex;
    private int prevGraphicIndex;

    public override Graphic Graphic => MizuGraphics.Buckets[graphicIndex]
        .GetColoredVersion(MizuGraphics.Buckets[graphicIndex].Shader, DrawColor, DrawColorTwo);

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        compWaterTool = GetComp<CompWaterTool>();
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref graphicIndex, "graphicIndex");
        prevGraphicIndex = graphicIndex;
    }

    public override void Tick()
    {
        base.Tick();

        prevGraphicIndex = graphicIndex;
        if (compWaterTool == null)
        {
            graphicIndex = 0;
            return;
        }

        for (var i = 0; i < graphicThreshold.Count; i++)
        {
            if (!(compWaterTool.StoredWaterVolumePercent < graphicThreshold[i]))
            {
                continue;
            }

            graphicIndex = i;
            break;
        }

        if (graphicIndex != prevGraphicIndex)
        {
            DirtyMapMesh(Map);
        }
    }
}