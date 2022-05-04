using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace MizuMod;

public class MapComponent_HiddenWaterSpot : MapComponent, ICellBoolGiver
{
    private const int RefreshInterval = 60000;

    private readonly CellBoolDrawer drawer;

    private readonly ushort[] spotGrid;

    private int allSpotNum;

    private int blockSizeX;

    private int blockSizeZ;

    private int lastUpdateTick;

    private HashSet<IntVec3> spotCells;

    public MapComponent_HiddenWaterSpot(Map map)
        : base(map)
    {
        spotGrid = new ushort[map.cellIndices.NumGridCells];
        spotCells = new HashSet<IntVec3>();
        drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 1f);
        lastUpdateTick = Find.TickManager.TicksGame;
    }

    public HashSet<IntVec3> SpotCells => spotCells;

    public Color Color => Color.white;

    public bool GetCellBool(int index)
    {
        return spotGrid[index] != 0;
    }

    public Color GetCellExtraColor(int index)
    {
        return spotGrid[index] != 0 ? new Color(1f, 0.5f, 0.5f, 0.5f) : new Color(1f, 1f, 1f, 0f);
    }

    public void CreateWaterSpot(int blockSizeX, int blockSizeZ, int allSpotNum)
    {
        ClearWaterSpot();

        this.blockSizeX = blockSizeX;
        this.blockSizeZ = blockSizeZ;
        this.allSpotNum = allSpotNum;

        var blockNumX = Mathf.CeilToInt((float)map.Size.x / 2 / blockSizeX);
        var blockNumZ = Mathf.CeilToInt((float)map.Size.z / 2 / blockSizeZ);
        var waterCellMap = new List<IntVec3>[blockNumX * 2, blockNumZ * 2];
        var allWaterNum = 0;

        for (var bx = -blockNumX; bx < blockNumX; bx++)
        {
            for (var bz = -blockNumZ; bz < blockNumZ; bz++)
            {
                waterCellMap[bx + blockNumX, bz + blockNumZ] = new List<IntVec3>();
                var waterCells = waterCellMap[bx + blockNumX, bz + blockNumZ];
                foreach (var c in new CellRect(
                             (bx * blockSizeX) + (map.Size.x / 2),
                             (bz * blockSizeZ) + (map.Size.z / 2),
                             blockSizeX,
                             blockSizeZ))
                {
                    if (!c.InBounds(map) || !c.GetTerrain(map).IsWaterStandable())
                    {
                        continue;
                    }

                    waterCells.Add(c);
                    allWaterNum++;
                }
            }
        }

        for (var bx = -blockNumX; bx < blockNumX; bx++)
        {
            for (var bz = -blockNumZ; bz < blockNumZ; bz++)
            {
                var waterCells = waterCellMap[bx + blockNumX, bz + blockNumZ];
                var spotNum = Mathf.Min(
                    Mathf.CeilToInt((float)waterCells.Count / allWaterNum * allSpotNum),
                    waterCells.Count);
                var randomCells = waterCells.InRandomOrder().ToList();
                for (var i = 0; i < spotNum; i++)
                {
                    spotGrid[map.cellIndices.CellToIndex(randomCells[i])] = 1;
                    spotCells.Add(randomCells[i]);
                }
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();

        MapExposeUtility.ExposeUshort(
            map,
            c => spotGrid[map.cellIndices.CellToIndex(c)],
            (c, id) => spotGrid[map.cellIndices.CellToIndex(c)] = id,
            "spotGrid");
        Scribe_Collections.Look(ref spotCells, "spotCells", LookMode.Value);
        Scribe_Values.Look(ref blockSizeX, "blockSizeX");
        Scribe_Values.Look(ref blockSizeZ, "blockSizeZ");
        Scribe_Values.Look(ref allSpotNum, "allSpotNum");
        Scribe_Values.Look(ref lastUpdateTick, "lastUpdateTick");

        if (!MizuDef.GlobalSettings.forDebug.enableResetHiddenWaterSpot)
        {
            return;
        }

        spotCells = new HashSet<IntVec3>();
        CreateWaterSpot(
            MizuDef.GlobalSettings.forDebug.resetHiddenWaterSpotBlockSizeX,
            MizuDef.GlobalSettings.forDebug.resetHiddenWaterSpotBlockSizeZ,
            MizuDef.GlobalSettings.forDebug.resetHiddenWaterSpotAllSpotNum);
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();

        if (lastUpdateTick + RefreshInterval > Find.TickManager.TicksGame)
        {
            return;
        }

        lastUpdateTick = Find.TickManager.TicksGame;
        CreateWaterSpot(blockSizeX, blockSizeZ, allSpotNum);
        SetDirty();
    }

    public override void MapComponentUpdate()
    {
        base.MapComponentUpdate();

        drawer.CellBoolDrawerUpdate();
    }

    public void MarkForDraw()
    {
        if (map == Find.CurrentMap)
        {
            drawer.MarkForDraw();
        }
    }

    private void ClearWaterSpot()
    {
        for (var i = 0; i < spotGrid.Length; i++)
        {
            spotGrid[i] = 0;
        }

        spotCells.Clear();
    }

    private void SetDirty()
    {
        if (map == Find.CurrentMap)
        {
            drawer.SetDirty();
        }
    }
}