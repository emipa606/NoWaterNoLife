using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class Building_Faucet : Building_WaterNetWorkTable, IBuilding_WaterNet, IBuilding_DrinkWater
    {
        public override void CreateConnectors()
        {
            InputConnectors.Clear();
            OutputConnectors.Clear();

            InputConnectors.Add(Position + Rotation.FacingCell);
            OutputConnectors.Add(Position + Rotation.FacingCell);
        }

        public WaterType WaterType
        {
            get
            {
                if (InputWaterNet == null)
                {
                    return WaterType.Undefined;
                }

                return InputWaterNet.StoredWaterType;
            }
        }

        public float WaterVolume
        {
            get
            {
                if (InputWaterNet == null)
                {
                    return 0f;
                }

                return InputWaterNet.StoredWaterVolume;
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (InputWaterNet == null)
                {
                    return true;
                }

                if (InputWaterNet.StoredWaterVolume <= 0f)
                {
                    return true;
                }

                return false;
            }
        }

        public bool CanDrinkFor(Pawn p)
        {
            if (p.needs == null || p.needs.Water() == null)
            {
                return false;
            }

            if (InputWaterNet == null)
            {
                return false;
            }

            if (InputWaterNet.StoredWaterTypeForFaucet == WaterType.Undefined || InputWaterNet.StoredWaterTypeForFaucet == WaterType.NoWater)
            {
                return false;
            }

            // 手が使用可能で、入力水道網の水量が十分にある
            return p.CanManipulate() && InputWaterNet.StoredWaterVolumeForFaucet >= p.needs.Water().WaterWanted * Need_Water.DrinkFromBuildingMargin;
        }

        public bool CanDrawFor(Pawn p)
        {
            if (InputWaterNet == null)
            {
                return false;
            }

            var targetWaterType = InputWaterNet.StoredWaterTypeForFaucet;
            if (targetWaterType == WaterType.Undefined || targetWaterType == WaterType.NoWater)
            {
                return false;
            }

            var waterItemDef = MizuDef.List_WaterItem.First((def) => def.GetCompProperties<CompProperties_WaterSource>().waterType == targetWaterType);
            var compprop = waterItemDef.GetCompProperties<CompProperties_WaterSource>();

            // 汲める予定の水アイテムの水の量より多い
            return p.CanManipulate() && InputWaterNet.StoredWaterVolumeForFaucet >= compprop.waterVolume;
        }

        public void DrawWater(float amount)
        {
            if (InputWaterNet == null)
            {
                return;
            }

            InputWaterNet.DrawWaterVolumeForFaucet(amount);
        }
    }
}
