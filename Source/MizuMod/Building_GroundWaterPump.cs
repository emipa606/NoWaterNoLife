namespace MizuMod
{
    public class Building_GroundWaterPump : Building_WaterNet
    {
        public override WaterType OutputWaterType => Map.terrainGrid.TerrainAt(Position).ToWaterType();
    }
}