using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Verse;
using RimWorld;

namespace MizuMod
{
    public class Building_SprinklerExtinguishing : Building_WaterNet, IBuilding_WaterNet
    {
        private CompPowerTrader compPowerTrader;

        private const float UseWaterVolumePerOne = 0.1f;
        private const int ExtinguishPower = 50;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPowerTrader = GetComp<CompPowerTrader>();
        }

        public override void TickRare()
        {
            base.TickRare();

            if (compPowerTrader.PowerOn)
            {
                // 電源ON、故障無し、稼働時間範囲内の時
                if (InputWaterNet != null)
                {
                    // 設備の置かれた部屋
                    var room = Position.GetRoom(Map);

                    // 部屋内もしくは隣接した火災
                    var fireList = room.ContainedAndAdjacentThings.Where((t) => t is Fire);

                    // 水やり範囲
                    var cells = GenRadial.RadialCellsAround(base.Position, def.specialDisplayRadius, true);

                    // 消火範囲内の部屋内火災or隣接火災
                    var targetFireList = fireList.Where((t) => cells.Contains(t.Position));

                    // 範囲内に火災があれば全域に水を撒く
                    if (targetFireList.Count() >= 1)
                    {
                        // 部屋内の水やり範囲
                        var roomCells = cells.Where((c) => c.GetRoom(Map) == room);

                        var targetFireCells = targetFireList.Select((t) => t.Position);

                        var wateringCells = roomCells.Union(targetFireCells);

                        // 水が足りているかチェック
                        var useWaterVolume = UseWaterVolumePerOne * wateringCells.Count();

                        if (InputWaterNet.StoredWaterVolumeForFaucet >= useWaterVolume)
                        {
                            var wateringComp = Map.GetComponent<MapComponent_Watering>();

                            InputWaterNet.DrawWaterVolumeForFaucet(useWaterVolume);

                            foreach (var fire in targetFireList)
                            {
                                // 消火効果(仮)
                                fire.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, ExtinguishPower));
                            }

                            foreach (var c in wateringCells)
                            {
                                // 水やりエフェクト(仮)
                                var mote = (MoteThrown)ThingMaker.MakeThing(MizuDef.Mote_SprinklerWater);
                                //mote.Scale = 1f;
                                //mote.rotationRate = (float)(Rand.Chance(0.5f) ? -30 : 30);
                                mote.exactPosition = c.ToVector3Shifted();
                                GenSpawn.Spawn(mote, c, base.Map);

                                // 水やり効果
                                if (Map.terrainGrid.TerrainAt(Map.cellIndices.CellToIndex(c)).fertility >= 0.01f)
                                {
                                    wateringComp.Add(Map.cellIndices.CellToIndex(c), 1);
                                    Map.mapDrawer.SectionAt(c).dirtyFlags = MapMeshFlag.Terrain;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
