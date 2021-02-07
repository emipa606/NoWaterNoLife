using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;

namespace MizuMod
{
    public class Building_Pipe : Building_WaterNet, IBuilding_WaterNet
    {
        public override Graphic Graphic
        {
            get
            {
                if (Position.GetTerrain(base.Map).layerable)
                {
                    return MizuGraphics.LinkedWaterPipeClear;
                }
                if (def.costStuffCount >= 0)
                {
                    return MizuGraphics.LinkedWaterPipe.GetColoredVersion(MizuGraphics.WaterPipe.Shader, DrawColor, DrawColorTwo);
                }
                return MizuGraphics.LinkedWaterPipe;
            }
        }
    }
}
