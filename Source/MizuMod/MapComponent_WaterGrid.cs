using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MizuMod;

public abstract class MapComponent_WaterGrid : MapComponent, ICellBoolGiver
{
    private CellBoolDrawer drawer;

    private int gridUpdates;

    private ushort[] poolIDGrid;

    private List<UndergroundWaterPool> pools = [];

    protected MapComponent_WaterGrid(Map map)
        : base(map)
    {
        poolIDGrid = new ushort[map.cellIndices.NumGridCells];
        drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 1f);
    }

    public Color Color => Color.white;

    public bool GetCellBool(int index)
    {
        return poolIDGrid[index] != 0;
    }

    public Color GetCellExtraColor(int index)
    {
        var pool = pools.Find(p => p.ID == poolIDGrid[index]);
        return UndergroundWaterMaterials.Mat(
            Mathf.RoundToInt(pool.CurrentWaterVolumePercent * UndergroundWaterMaterials.MaterialCount)).color;
    }

    public void AddWaterPool(UndergroundWaterPool pool, IEnumerable<IntVec3> cells)
    {
        var mergePools = new List<UndergroundWaterPool> { pool };

        // 既存の水源と被るセルを調べる
        foreach (var c in cells)
        {
            if (GetID(c) == 0)
            {
                continue;
            }

            var existPool = pools.Find(p => p.ID == GetID(c));
            if (existPool == null)
            {
                Log.Message("[NoWaterNoLife]: Found no pool");
            }

            if (!mergePools.Contains(existPool))
            {
                mergePools.Add(existPool);
            }
        }

        // 上書き覚悟でとりあえず追加
        pools.Add(pool);
        foreach (var c in cells)
        {
            SetID(c, pool.ID);
        }

        if (mergePools.Count < 2)
        {
            return;
        }

        {
            // 最小の水源IDのものに統合する

            // 最小の水源IDを調べる
            UndergroundWaterPool minPool = null;
            foreach (var p in mergePools)
            {
                if (minPool == null || minPool.ID > p.ID)
                {
                    minPool = p;
                }
            }

            // 統合される水源から最小IDのものを除き、消滅予定水源リストに変換
            mergePools.Remove(minPool);

            // 水源リストから消滅予定水源を除去しつつ水量を統合
            foreach (var p in mergePools)
            {
                pools.Remove(p);
                minPool?.MergeWaterVolume(p);
            }

            // 全セルを調べ、消滅予定水源IDの場所を最小IDに変更
            for (var i = 0; i < poolIDGrid.Length; i++)
            {
                // Log.Message("i=" + i.ToString());
                if (mergePools.Find(p => p.ID == GetID(i)) == null)
                {
                    continue;
                }

                if (minPool != null)
                {
                    SetID(i, minPool.ID);
                }
            }
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();

        MapExposeUtility.ExposeUshort(
            map,
            c => poolIDGrid[map.cellIndices.CellToIndex(c)],
            (c, id) => poolIDGrid[map.cellIndices.CellToIndex(c)] = id,
            "poolIDGrid");

        Scribe_Collections.Look(ref pools, "pools", LookMode.Deep, this);
    }

    public int GetID(int index)
    {
        return poolIDGrid[index];
    }

    public int GetID(IntVec3 c)
    {
        return GetID(map.cellIndices.CellToIndex(c));
    }

    public UndergroundWaterPool GetPool(int index)
    {
        return pools.Find(p => p.ID == poolIDGrid[index]);
    }

    public override void MapComponentUpdate()
    {
        base.MapComponentUpdate();

        drawer.CellBoolDrawerUpdate();
        var anyPools = false;
        foreach (var pool in pools)
        {
            if (pool == null)
            {
                continue;
            }

            pool.Update();
            anyPools = true;
        }

        if (anyPools)
        {
            return;
        }

        if (gridUpdates > 20)
        {
            if (gridUpdates >= 50)
            {
                return;
            }

            Log.Message("No water no life: Water grid not found, and failed to regenerate after 20 tries");
            gridUpdates = 55;
            return;
        }

        gridUpdates++;
        poolIDGrid = new ushort[map.cellIndices.NumGridCells];
        drawer = new CellBoolDrawer(this, map.Size.x, map.Size.z, 1f);
        pools = [];
        MizuUtility.GenerateUndergroundWaterGrid(map, this);
    }

    public void MarkForDraw()
    {
        if (map == Find.CurrentMap)
        {
            drawer.MarkForDraw();
        }
    }

    public void ModifyPoolGrid()
    {
        var nearVecs = new List<IntVec3>();
        for (var x = 0; x < map.Size.x; x++)
        {
            for (var z = 0; z < map.Size.z; z++)
            {
                var curIndex = map.cellIndices.CellToIndex(new IntVec3(x, 0, z));
                if (GetID(curIndex) == 0)
                {
                    continue;
                }

                nearVecs.Clear();
                if (x - 1 >= 0)
                {
                    nearVecs.Add(new IntVec3(x - 1, 0, z));
                }

                if (x + 1 < map.Size.x)
                {
                    nearVecs.Add(new IntVec3(x + 1, 0, z));
                }

                if (z - 1 >= 0)
                {
                    nearVecs.Add(new IntVec3(x, 0, z - 1));
                }

                if (z + 1 < map.Size.z)
                {
                    nearVecs.Add(new IntVec3(x, 0, z + 1));
                }

                foreach (var nearVec in nearVecs)
                {
                    var nearIndex = map.cellIndices.CellToIndex(nearVec);
                    if (GetID(nearIndex) == 0)
                    {
                        continue;
                    }

                    if (GetID(curIndex) == GetID(nearIndex))
                    {
                        continue;
                    }

                    var curPool = pools.Find(p => p.ID == GetID(curIndex));
                    if (curPool == null)
                    {
                        Log.Message("[NoWaterNoLife]: Found no current pool");
                    }

                    var nearPool = pools.Find(p => p.ID == GetID(nearIndex));
                    if (nearPool == null)
                    {
                        Log.Message("[NoWaterNoLife]: Found no near pool");
                    }

                    curPool?.MergePool(nearPool, poolIDGrid);

                    pools.Remove(nearPool);

                    break;
                }
            }
        }
    }

    public void SetDirty()
    {
        if (map == Find.CurrentMap)
        {
            drawer.SetDirty();
        }
    }

    private void SetID(int index, int id)
    {
        poolIDGrid[index] = (ushort)id;
    }

    private void SetID(IntVec3 c, int id)
    {
        SetID(map.cellIndices.CellToIndex(c), id);
    }
}