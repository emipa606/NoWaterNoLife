using System.Text;
using Verse;

namespace MizuMod;

public abstract class Building_UndergroundWaterPump : Building_WaterNet
{
    private UndergroundWaterPool pool;

    public override WaterType OutputWaterType => pool.WaterType;

    public override UndergroundWaterPool WaterPool => pool;

    protected abstract MapComponent_WaterGrid WaterGrid { get; }

    public override string GetInspectString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(base.GetInspectString());

        if (stringBuilder.ToString() != string.Empty)
        {
            stringBuilder.AppendLine();
        }

        if (pool != null)
        {
            stringBuilder.Append(
                string.Format(
                    MizuStrings.InspectStoredWaterPool.Translate() + ": {0}%",
                    (pool.CurrentWaterVolumePercent * 100).ToString("F0")));
        }

        if (DebugSettings.godMode)
        {
            stringBuilder.Append($" ({pool?.CurrentWaterVolume:F2}/{pool?.MaxWaterVolume:F2} L)");
        }

        return stringBuilder.ToString().Trim();
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        pool = WaterGrid.GetPool(map.cellIndices.CellToIndex(Position));
        if (pool == null)
        {
            Log.Message("[NoWaterNoLife]: Found no pool, no water will be available");
        }
    }
}