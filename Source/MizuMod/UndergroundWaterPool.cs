using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;

namespace MizuMod
{
    public class UndergroundWaterPool : IExposable
    {
        private bool debugFlag = true;

        public int ID;

        private WaterType waterType;
        public WaterType WaterType => waterType;

        private float maxWaterVolume;
        public float MaxWaterVolume => maxWaterVolume;
        private float currentWaterVolume;
        public float CurrentWaterVolume
        {
            get => currentWaterVolume;
            set
            {
                currentWaterVolume = Mathf.Max(0, Mathf.Min(maxWaterVolume, value));
                var curMaterialIndex = Mathf.RoundToInt(CurrentWaterVolumePercent * UndergroundWaterMaterials.MaterialCount);
                if (lastMaterialIndex != curMaterialIndex)
                {
                    lastMaterialIndex = curMaterialIndex;
                    waterGrid.SetDirty();
                }
            }
        }
        private float baseRegenRate;
        public float BaseRegenRate => baseRegenRate;
        private float rainRegenRatePerCell;
        public float RainRegenRatePerCell => rainRegenRatePerCell;
        private float outputWaterFlow;
        public float OutputWaterFlow
        {
            get => outputWaterFlow;
            set => outputWaterFlow = Mathf.Max(value, 0f);
        }
        private int lastTick;

        private int lastMaterialIndex = UndergroundWaterMaterials.MaterialCount;

        public float CurrentWaterVolumePercent => CurrentWaterVolume / maxWaterVolume;

        private readonly MapComponent_WaterGrid waterGrid;

        private List<IntVec3> poolCells = null;

        public UndergroundWaterPool(MapComponent_WaterGrid waterGrid)
        {
            this.waterGrid = waterGrid;
            lastTick = Find.TickManager.TicksGame;
        }

        public UndergroundWaterPool(MapComponent_WaterGrid waterGrid, float maxWaterVolume, WaterType waterType, float baseRegenRate, float rainRegenRatePerCell) : this(waterGrid)
        {
            this.maxWaterVolume = maxWaterVolume;
            currentWaterVolume = maxWaterVolume;
            this.waterType = waterType;
            this.baseRegenRate = baseRegenRate;
            this.rainRegenRatePerCell = rainRegenRatePerCell;
        }

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref ID, "ID");
            Scribe_Values.Look<float>(ref maxWaterVolume, "maxWaterVolume");
            Scribe_Values.Look<float>(ref currentWaterVolume, "currenteWaterVolume");
            Scribe_Values.Look<WaterType>(ref waterType, "waterType");
            Scribe_Values.Look<float>(ref baseRegenRate, "baseRegenRate");
            Scribe_Values.Look<float>(ref rainRegenRatePerCell, "rainRegenRatePerCell");
            Scribe_Values.Look<int>(ref lastTick, "lastTick");
            Scribe_Values.Look<float>(ref outputWaterFlow, "outputWaterFlow");

            if (debugFlag)
            {
                debugFlag = false;
                if (MizuDef.GlobalSettings.forDebug.enableChangeWaterPoolType)
                {
                    waterType = MizuDef.GlobalSettings.forDebug.changeWaterPoolType;
                }
                if (MizuDef.GlobalSettings.forDebug.enableChangeWaterPoolVolume)
                {
                    maxWaterVolume *= MizuDef.GlobalSettings.forDebug.waterPoolVolumeRate;
                    currentWaterVolume *= MizuDef.GlobalSettings.forDebug.waterPoolVolumeRate;
                }
                if (MizuDef.GlobalSettings.forDebug.enableResetRegenRate)
                {
                    if (waterGrid is MapComponent_ShallowWaterGrid)
                    {
                        baseRegenRate = MizuDef.GlobalSettings.forDebug.resetBaseRegenRateRangeForShallow.RandomInRange;
                        rainRegenRatePerCell = MizuDef.GlobalSettings.forDebug.resetRainRegenRatePerCellForShallow;
                    }
                    if (waterGrid is MapComponent_DeepWaterGrid)
                    {
                        baseRegenRate = MizuDef.GlobalSettings.forDebug.resetBaseRegenRateRangeForDeep.RandomInRange;
                        rainRegenRatePerCell = MizuDef.GlobalSettings.forDebug.resetRainRegenRatePerCellForDeep;
                    }
                }
            }
        }

        public void MergeWaterVolume(UndergroundWaterPool p)
        {
            maxWaterVolume += p.maxWaterVolume;
            CurrentWaterVolume += p.CurrentWaterVolume;
        }

        public void MergePool(UndergroundWaterPool p, ushort[] idGrid)
        {
            MergeWaterVolume(p);
            for (var i = 0; i < idGrid.Length; i++)
            {
                if (idGrid[i] == p.ID)
                {
                    idGrid[i] = (ushort)ID;
                }
            }
        }

        public void Update()
        {
            if (poolCells == null)
            {
                GeneratePoolCells();
            }

            var curTick = Find.TickManager.TicksGame;

            // 基本回復量
            var addWaterVolumeBase = baseRegenRate / 60000.0f * (curTick - lastTick);

            // 屋根チェック
            var unroofedCells = 0;
            foreach (var c in poolCells)
            {
                if (!c.Roofed(waterGrid.map))
                {
                    unroofedCells++;
                }
            }

            // 雨による回復量
            var addWaterVolumeRain = rainRegenRatePerCell * unroofedCells / 60000.0f * waterGrid.map.weatherManager.RainRate * (curTick - lastTick);

            // 合計回復量
            var addWaterVolumeTotal = addWaterVolumeBase + addWaterVolumeRain;
            if (addWaterVolumeTotal < 0.0f)
            {
                addWaterVolumeTotal = 0.0f;
            }

            // 出力量(吸い上げられる量)との差分
            var deltaWaterVolume = addWaterVolumeTotal - (outputWaterFlow / 60000.0f);

            // 差分を加算
            CurrentWaterVolume = Math.Min(CurrentWaterVolume + deltaWaterVolume, MaxWaterVolume);

            // 設定された量を減らしたのでクリア
            outputWaterFlow = 0f;

            lastTick = curTick;
        }

        private void GeneratePoolCells()
        {
            poolCells = new List<IntVec3>();

            foreach (var c in waterGrid.map.AllCells)
            {
                if (ID == waterGrid.GetID(c))
                {
                    poolCells.Add(c);
                }
            }
        }
    }
}
