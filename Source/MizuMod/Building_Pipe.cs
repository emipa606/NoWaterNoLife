using Verse;

namespace MizuMod
{
    public class Building_Pipe : Building_WaterNet
    {
        public override Graphic Graphic
        {
            get
            {
                if (Position.GetTerrain(Map).layerable)
                {
                    return MizuGraphics.LinkedWaterPipeClear;
                }

                if (def.costStuffCount >= 0)
                {
                    return MizuGraphics.LinkedWaterPipe.GetColoredVersion(
                        MizuGraphics.WaterPipe.Shader,
                        DrawColor,
                        DrawColorTwo);
                }

                return MizuGraphics.LinkedWaterPipe;
            }
        }
    }
}