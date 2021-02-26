using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace MizuMod
{
    public class UndergroundWaterPool : IExposable
    {
        private readonly MapComponent_WaterGrid waterGrid;
        private float baseRegenRate;
        private float currentWaterVolume;
        private bool debugFlag = true;

        public int ID;

        private int lastMaterialIndex = UndergroundWaterMaterials.MaterialCount;
        private int lastTick;

        private float maxWaterVolume;
        private float outputWaterFlow;

        private List<IntVec3> poolCells;
        private float rainRegenRatePerCell;

        private WaterType waterType;

        // ReSharper disable once MemberCanBePrivate.Global
        // Needs to be public for the water-grid loading to work
        public UndergroundWaterPool(MapComponent_WaterGrid waterGrid)
        {
            this.waterGrid = waterGrid;
            lastTick = Find.TickManager.TicksGame;
        }

        public UndergroundWaterPool(MapComponent_WaterGrid waterGrid, float maxWaterVolume, WaterType waterType,
            float baseRegenRate, float rainRegenRatePerCell) : this(waterGrid)
        {
            this.maxWaterVolume = maxWaterVolume;
            currentWaterVolume = maxWaterVolume;
            this.waterType = waterType;
            this.baseRegenRate = baseRegenRate;
            this.rainRegenRatePerCell = rainRegenRatePerCell;
        }

        public WaterType WaterType => waterType;
        public float MaxWaterVolume => maxWaterVolume;

        public float CurrentWaterVolume
        {
            get => currentWaterVolume;
            set
            {
                currentWaterVolume = Mathf.Max(0, Mathf.Min(maxWaterVolume, value));
                var curMaterialIndex =
                    Mathf.RoundToInt(CurrentWaterVolumePercent * UndergroundWaterMaterials.MaterialCount);
                if (lastMaterialIndex == curMaterialIndex)
                {
                    return;
                }

                lastMaterialIndex = curMaterialIndex;
                waterGrid.SetDirty();
            }
        }


        public float BaseRegenRate => baseRegenRate;
        public float RainRegenRatePerCell => rainRegenRatePerCell;

        public float OutputWaterFlow
        {
            get => outputWaterFlow;
            set => outputWaterFlow = Mathf.Max(value, 0f);
        }

        public float CurrentWaterVolumePercent => CurrentWaterVolume / maxWaterVolume;

        public void ExposeData()
        {
            Scribe_Values.Look(ref ID, "ID");
            Scribe_Values.Look(ref maxWaterVolume, "maxWaterVolume");
            Scribe_Values.Look(ref currentWaterVolume, "currenteWaterVolume");
            Scribe_Values.Look(ref waterType, "waterType");
            Scribe_Values.Look(ref baseRegenRate, "baseRegenRate");
            Scribe_Values.Look(ref rainRegenRatePerCell, "rainRegenRatePerCell");
            Scribe_Values.Look(ref lastTick, "lastTick");
            Scribe_Values.Look(ref outputWaterFlow, "outputWaterFlow");

            if (!debugFlag)
            {
                return;
            }

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

            if (!MizuDef.GlobalSettings.forDebug.enableResetRegenRate)
            {
                return;
            }

            if (waterGrid is MapComponent_ShallowWaterGrid)
            {
                baseRegenRate = MizuDef.GlobalSettings.forDebug.resetBaseRegenRateRangeForShallow.RandomInRange;
                rainRegenRatePerCell = MizuDef.GlobalSettings.forDebug.resetRainRegenRatePerCellForShallow;
            }

            if (!(waterGrid is MapComponent_DeepWaterGrid))
            {
                return;
            }

            baseRegenRate = MizuDef.GlobalSettings.forDebug.resetBaseRegenRateRangeForDeep.RandomInRange;
            rainRegenRatePerCell = MizuDef.GlobalSettings.forDebug.resetRainRegenRatePerCellForDeep;
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
                    idGrid[i] = (ushort) ID;
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
            var addWaterVolumeRain = rainRegenRatePerCell * unroofedCells / 60000.0f *
                                     waterGrid.map.weatherManager.RainRate * (curTick - lastTick);

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