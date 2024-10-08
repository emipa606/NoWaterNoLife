﻿using System.Linq;
using Verse;

namespace MizuMod;

public class Building_Faucet : Building_WaterNetWorkTable, IBuilding_DrinkWater
{
    public bool IsEmpty
    {
        get
        {
            if (InputWaterNet == null)
            {
                return true;
            }

            return InputWaterNet.StoredWaterVolume <= 0f;
        }
    }

    public WaterType WaterType => InputWaterNet?.StoredWaterType ?? WaterType.Undefined;

    public float WaterVolume => InputWaterNet?.StoredWaterVolume ?? 0f;

    public bool CanDrawFor(Pawn p)
    {
        if (InputWaterNet == null)
        {
            return false;
        }

        var targetWaterType = InputWaterNet.StoredWaterTypeForFaucet;
        if (targetWaterType is WaterType.Undefined or WaterType.NoWater)
        {
            return false;
        }

        var waterItemDef = MizuDef.List_WaterItem.First(
            thingDef => thingDef.GetCompProperties<CompProperties_WaterSource>().waterType == targetWaterType);
        var compprop = waterItemDef.GetCompProperties<CompProperties_WaterSource>();

        // 汲める予定の水アイテムの水の量より多い
        return p.CanManipulate() && InputWaterNet.StoredWaterVolumeForFaucet >= compprop.waterVolume;
    }

    public bool CanDrinkFor(Pawn p)
    {
        if (p.needs?.Water() == null)
        {
            return false;
        }

        if (InputWaterNet == null)
        {
            return false;
        }

        if (InputWaterNet.StoredWaterTypeForFaucet is WaterType.Undefined or WaterType.NoWater)
        {
            return false;
        }

        // 手が使用可能で、入力水道網の水量が十分にある
        return p.CanManipulate() && InputWaterNet.StoredWaterVolumeForFaucet
            >= p.needs.Water().WaterWanted * Need_Water.DrinkFromBuildingMargin;
    }

    public void DrawWater(float amount)
    {
        InputWaterNet?.DrawWaterVolumeForFaucet(amount);
    }

    public override void CreateConnectors()
    {
        InputConnectors.Clear();
        OutputConnectors.Clear();

        InputConnectors.Add(Position + Rotation.FacingCell);
        OutputConnectors.Add(Position + Rotation.FacingCell);
    }
}