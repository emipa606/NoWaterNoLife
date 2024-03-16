using System.Linq;
using RimWorld;
using Verse;

namespace MizuMod;

public class Building_SprinklerExtinguishing : Building_WaterNet
{
    private const int ExtinguishPower = 50;

    private const float UseWaterVolumePerOne = 0.1f;

    private CompPowerTrader compPowerTrader;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        compPowerTrader = GetComp<CompPowerTrader>();
    }

    public override void TickRare()
    {
        base.TickRare();

        if (!compPowerTrader.PowerOn)
        {
            return;
        }

        // 電源ON、故障無し、稼働時間範囲内の時
        if (InputWaterNet == null)
        {
            return;
        }

        // 設備の置かれた部屋
        var room = Position.GetRoom(Map);

        // 部屋内もしくは隣接した火災
        var fireList = room.ContainedAndAdjacentThings.Where(t => t is Fire);

        // 水やり範囲
        var cells = GenRadial.RadialCellsAround(Position, def.specialDisplayRadius, true);

        // 消火範囲内の部屋内火災or隣接火災
        var targetFireList = fireList.Where(t => cells.Contains(t.Position));

        // 範囲内に火災があれば全域に水を撒く
        var enumerable = targetFireList as Thing[] ?? targetFireList.ToArray();
        if (enumerable.Length < 1)
        {
            return;
        }

        // 部屋内の水やり範囲
        var roomCells = cells.Where(c => c.GetRoom(Map) == room);

        var targetFireCells = enumerable.Select(t => t.Position);

        var wateringCells = roomCells.Union(targetFireCells);

        // 水が足りているかチェック
        var useWaterVolume = UseWaterVolumePerOne * wateringCells.Count();

        if (!(InputWaterNet.StoredWaterVolumeForFaucet >= useWaterVolume))
        {
            return;
        }

        var wateringComp = Map.GetComponent<MapComponent_Watering>();

        InputWaterNet.DrawWaterVolumeForFaucet(useWaterVolume);

        foreach (var fire in enumerable)
        {
            // 消火効果(仮)
            fire.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, ExtinguishPower));
        }

        foreach (var c in wateringCells)
        {
            // 水やりエフェクト(仮)
            var mote = (MoteThrown)ThingMaker.MakeThing(MizuDef.Mote_SprinklerWater);

            // mote.Scale = 1f;
            // mote.rotationRate = (float)(Rand.Chance(0.5f) ? -30 : 30);
            mote.exactPosition = c.ToVector3Shifted();
            GenSpawn.Spawn(mote, c, Map);

            // 水やり効果
            if (!(Map.terrainGrid.TerrainAt(Map.cellIndices.CellToIndex(c)).fertility >= 0.01f))
            {
                continue;
            }

            wateringComp.Add(Map.cellIndices.CellToIndex(c), 1);
            Map.mapDrawer.SectionAt(c).dirtyFlags = MapMeshFlagDefOf.Terrain;
        }
    }
}