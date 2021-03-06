﻿using Verse;

namespace MizuMod
{
    public class DefExtension_NeedWater : DefModExtension
    {
        public float dehydrationSeverityPerDay = 0.1f;

        public float fallPerTickBase = 1.33E-05f;

        public SimpleCurve fallPerTickFromTempCurve = null;

        public float slightlyThirstyBorder = 0.6f;

        public float thirstyBorder = 0.24f;

        public float urgentlyThirstyBorder = 0.12f;

        // public SimpleCurve fallPerTickFromTempCurve = new SimpleCurve
        // {
        // new CurvePoint(30f, 0f),
        // new CurvePoint(130f, 1.5E-05f),
        // };
    }
}