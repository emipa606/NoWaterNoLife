﻿using System.Collections.Generic;
using RimWorld;
using Verse;

namespace MizuMod;

public class CompProperties_WaterSource : CompProperties
{
    public enum SourceType : byte
    {
        Undefined = 0,

        Item,

        Building
    }

    // 飲むのにかかるTick
    // 水アイテムの場合、1個あたりのTick
    // 設備の場合、要求を1.0得るのにかかるTick
    public readonly int baseDrinkTicks = 100;

    // 水アイテム用 水質が使用材料に依存するか
    public readonly bool dependIngredients = false;

    // 水設備用 水抜き速度
    public readonly float drainWaterFlow = 1000.0f;

    // 飲むときのエフェクト
    public readonly EffecterDef getEffect = null;

    // 飲むときの音
    public readonly SoundDef getSound = null;

    // 水アイテム用 1回に摂取できる最大数
    public readonly int maxNumToGetAtOnce = 1;

    // 水を飲むのに手を必要とするかどうか
    public readonly bool needManipulate = false;

    // どのタイプの水源か
    public readonly SourceType sourceType = SourceType.Undefined;

    // 水アイテム用 1個あたりの水の量(Need換算)
    public readonly float waterAmount = 0.0f;

    // 水アイテム用 水質
    public readonly WaterType waterType = WaterType.Undefined;

    // 水アイテム用 1個あたりの水の量(リットル換算)
    public readonly float waterVolume = 0.0f;

    public CompProperties_WaterSource()
    {
        compClass = typeof(CompWaterSource);
    }

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
    {
        foreach (var statDrawEntry in base.SpecialDisplayStats(req))
        {
            yield return statDrawEntry;
        }

        if (sourceType == SourceType.Item)
        {
            // アイテム1個から得られる水分量
            yield return new StatDrawEntry(
                MizuDef.StatCategory_Water,
                StatDef.Named("Mizu_DrawingSpeed"),
                waterAmount,
                req,
                ToStringNumberSense.Undefined,
                11);
        }
    }
}