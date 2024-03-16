using System;
using Verse;

namespace MizuMod;

public class CompProperties_LatentHeat(Type compClass) : CompProperties(compClass)
{
    public enum AddCondition : byte
    {
        Undefined = 0,

        Above,

        Below
    }

    // 温度閾値を上回ったら増加するのか、下回ったら増加するのか
    public readonly AddCondition addLatentHeatCondition = AddCondition.Undefined;

    // 潜熱が溜まると何に変化するのか
    // (nullは消滅)
    public readonly ThingDef changedThingDef = null;

    // 潜熱閾値
    public readonly float latentHeatThreshold = 1f;

    // 温度閾値
    public readonly float temperatureThreshold = 0f;

    public CompProperties_LatentHeat()
        : this(typeof(CompLatentHeat))
    {
    }
}