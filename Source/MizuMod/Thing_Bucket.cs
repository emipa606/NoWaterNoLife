using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class Thing_Bucket : ThingWithComps
    {
        private readonly List<float> graphicThreshold = new List<float>()
        {
            0.9f,
            100f,
        };

        private int graphicIndex = 0;
        private int prevGraphicIndex = 0;

        public override Graphic Graphic => MizuGraphics.Buckets[graphicIndex].GetColoredVersion(MizuGraphics.Buckets[graphicIndex].Shader, DrawColor, DrawColorTwo);

        private CompWaterTool compWaterTool;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            compWaterTool = GetComp<CompWaterTool>();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look<int>(ref graphicIndex, "graphicIndex");
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
                if (compWaterTool.StoredWaterVolumePercent < graphicThreshold[i])
                {
                    graphicIndex = i;
                    break;
                }
            }

            if (graphicIndex != prevGraphicIndex)
            {
                DirtyMapMesh(Map);
            }
        }
    }
}
