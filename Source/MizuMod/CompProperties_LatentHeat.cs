﻿using System;
using Verse;

namespace MizuMod
{
    public class CompProperties_LatentHeat : CompProperties
    {
        public enum AddCondition : byte
        {
            Undefined = 0,
            Above,
            Below
        }

        // 温度閾値を上回ったら増加するのか、下回ったら増加するのか
        public AddCondition addLatentHeatCondition = AddCondition.Undefined;

        // 潜熱が溜まると何に変化するのか
        // (nullは消滅)
        public ThingDef changedThingDef = null;

        // 潜熱閾値
        public float latentHeatThreshold = 1f;

        // 温度閾値
        public float temperatureThreshold = 0f;

        public CompProperties_LatentHeat() : base(typeof(CompLatentHeat))
        {
        }

        public CompProperties_LatentHeat(Type compClass) : base(compClass)
        {
        }
    }
}