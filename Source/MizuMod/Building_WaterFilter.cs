using Verse;

namespace MizuMod;

public class Building_WaterFilter : Building_WaterNet
{
    public override bool IsSameConnector => false;

    public override void CreateConnectors()
    {
        InputConnectors.Clear();
        OutputConnectors.Clear();

        if (def.Size == IntVec2.One)
        {
            InputConnectors.Add(Position + (Rotation.FacingCell * -1));
            OutputConnectors.Add(Position + Rotation.FacingCell);
            return;
        }

        if (def.Size != IntVec2.Two)
        {
            return;
        }

        InputConnectors.Add(Position + (Rotation.FacingCell * -1));
        InputConnectors.Add(Position + (Rotation.FacingCell * -1) + Rotation.RighthandCell);
        OutputConnectors.Add(Position + (Rotation.FacingCell * 2));
        OutputConnectors.Add(Position + (Rotation.FacingCell * 2) + Rotation.RighthandCell);
    }
}