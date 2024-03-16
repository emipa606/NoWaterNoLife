using RimWorld;
using Verse;

namespace MizuMod;

public class MapComponent_Watering(Map map) : MapComponent(map)
{
    public const ushort MaxWateringValue = 10;

    private const int IntervalTicks = 250;

    // 水やり効果がなくなるまでの残りTick
    private readonly ushort[] wateringGrid = new ushort[map.cellIndices.NumGridCells];

    private int elapsedTicks;

    private int randomIndex;

    public void Add(int index, ushort val)
    {
        Set(index, (ushort)(Get(index) + val));
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref elapsedTicks, "elapsedTicks");
        Scribe_Values.Look(ref randomIndex, "randomIndex");
        MapExposeUtility.ExposeUshort(
            map,
            c => wateringGrid[map.cellIndices.CellToIndex(c)],
            (c, val) => wateringGrid[map.cellIndices.CellToIndex(c)] = val,
            "wateringGrid");
    }

    public ushort Get(int index)
    {
        return wateringGrid[index];
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();

        elapsedTicks++;
        if (elapsedTicks < IntervalTicks)
        {
            return;
        }

        elapsedTicks = 0;

        var numCells = map.Area * IntervalTicks / 60000 * 10;

        for (var i = 0; i < numCells; i++)
        {
            var c = map.cellsInRandomOrder.Get(randomIndex);
            var terrain = map.terrainGrid.TerrainAt(c);
            if (map.weatherManager.RainRate > 0.5f && !map.roofGrid.Roofed(c) && terrain.fertility >= 0.01f)
            {
                // 雨が降れば水やり効果
                wateringGrid[map.cellIndices.CellToIndex(c)] = 10;
                map.mapDrawer.SectionAt(c).dirtyFlags = MapMeshFlagDefOf.Terrain;
            }
            else if (wateringGrid[map.cellIndices.CellToIndex(c)] > 0)
            {
                // 水が渇く
                wateringGrid[map.cellIndices.CellToIndex(c)]--;
                if (wateringGrid[map.cellIndices.CellToIndex(c)] == 0)
                {
                    map.mapDrawer.SectionAt(c).dirtyFlags = MapMeshFlagDefOf.Terrain;
                }
            }

            randomIndex++;
            if (randomIndex >= map.cellIndices.NumGridCells)
            {
                randomIndex = 0;
            }
        }
    }

    public void Set(int index, ushort val)
    {
        wateringGrid[index] = val < MaxWateringValue ? val : MaxWateringValue;
    }
}