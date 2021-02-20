namespace MizuMod
{
    public class Building_WaterFilter : Building_WaterNet
    {
        public override bool IsSameConnector => false;

        public override void CreateConnectors()
        {
            InputConnectors.Clear();
            OutputConnectors.Clear();

            InputConnectors.Add(Position + (Rotation.FacingCell * -1));
            OutputConnectors.Add(Position + Rotation.FacingCell);
        }
    }
}