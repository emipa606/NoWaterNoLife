﻿using Verse;

namespace MizuMod;

public class Verb_LaunchBucketWater : Verb_LaunchProjectile
{
    private const float NeedWaterPercentage = 0.9f;

    protected override bool TryCastShot()
    {
        // 水チェック
        var comp = EquipmentSource.GetComp<CompWaterTool>();
        if (comp.StoredWaterVolumePercent < NeedWaterPercentage)
        {
            return false;
        }

        // 通常の投擲チェック
        if (base.TryCastShot() == false)
        {
            return false;
        }

        // 投擲できた場合は水を減らす
        comp.StoredWaterVolume = 0f;

        // グラフィック更新
        EquipmentSource.Tick();

        return true;
    }
}