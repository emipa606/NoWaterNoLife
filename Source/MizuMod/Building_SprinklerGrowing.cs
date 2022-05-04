using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace MizuMod;

public class Building_SprinklerGrowing : Building_WaterNet
{
    private const int ExtinguishPower = 50;

    private const float UseWaterVolumePerOne = 0.1f;

    private CompPowerTrader compPowerTrader;

    private CompSchedule compSchedule;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        compPowerTrader = GetComp<CompPowerTrader>();
        compSchedule = GetComp<CompSchedule>();

        ResetPowerOutput();
    }

    public override void TickRare()
    {
        base.TickRare();

        // デバッグオプションがONなら時間設定や電力状態を無視
        if (compPowerTrader.PowerOn || MizuDef.GlobalSettings.forDebug.enableAlwaysActivateSprinklerGrowing)
        {
            // 電源ON、故障無し、稼働時間範囲内の時
            if (InputWaterNet != null)
            {
                // 水やり範囲
                var cells = GenRadial.RadialCellsAround(Position, def.specialDisplayRadius, true);

                // 設備の置かれた部屋
                var room = Position.GetRoom(Map);

                // 設備と同じ部屋に属するセル(肥沃度あり)
                // 暫定で植木鉢は無効とする
                var sameRoomCells = cells.Where(
                    c => c.GetRoom(Map) == room && Map.terrainGrid.TerrainAt(c).fertility >= 0.01f);

                var wateringComp = Map.GetComponent<MapComponent_Watering>();

                // 10の水やり効果で1L→1の水やり効果で0.1L
                // 水が足りているかチェック
                var useWaterVolume = UseWaterVolumePerOne * sameRoomCells.Count();

                // デバッグオプションがONなら消費貯水量を0.1Lにする
                if (MizuDef.GlobalSettings.forDebug.enableAlwaysActivateSprinklerGrowing)
                {
                    useWaterVolume = 0.1f;
                }

                if (InputWaterNet.StoredWaterVolumeForFaucet >= useWaterVolume)
                {
                    // 水を減らしてからセルに水やり効果
                    InputWaterNet.DrawWaterVolumeForFaucet(useWaterVolume);
                    foreach (var c in sameRoomCells)
                    {
                        wateringComp.Add(Map.cellIndices.CellToIndex(c), 1);
                        Map.mapDrawer.SectionAt(c).dirtyFlags = MapMeshFlag.Terrain;

                        // 水やりエフェクト(仮)
                        var mote = (MoteThrown)ThingMaker.MakeThing(MizuDef.Mote_SprinklerWater);

                        // mote.Scale = 1f;
                        // mote.rotationRate = (float)(Rand.Chance(0.5f) ? -30 : 30);
                        mote.exactPosition = c.ToVector3Shifted();
                        GenSpawn.Spawn(mote, c, Map);

                        // 消火効果(仮)
                        // 複製しないとダメージを受けて消えた時点で元のリストから除外されてエラーになる
                        var fireList = new List<Fire>(Map.thingGrid.ThingsListAt(c).OfType<Fire>());
                        foreach (var fire in fireList)
                        {
                            fire.TakeDamage(new DamageInfo(DamageDefOf.Extinguish, ExtinguishPower));
                        }
                    }
                }
            }
        }

        ResetPowerOutput();
    }

    private void ResetPowerOutput()
    {
        if (compSchedule.Allowed)
        {
            // 稼働中の消費電力
            compPowerTrader.PowerOutput = -compPowerTrader.Props.basePowerConsumption;
        }
        else
        {
            // 非稼働時の消費電力
            compPowerTrader.PowerOutput = -compPowerTrader.Props.basePowerConsumption * 0.1f;
        }
    }
}