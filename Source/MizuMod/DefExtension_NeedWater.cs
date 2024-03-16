using Verse;

namespace MizuMod;

public class DefExtension_NeedWater : DefModExtension
{
    public readonly float dehydrationSeverityPerDay = 0.1f;

    public readonly float fallPerTickBase = 1.33E-05f;

    public readonly SimpleCurve fallPerTickFromTempCurve = null;

    public readonly float slightlyThirstyBorder = 0.6f;

    public readonly float thirstyBorder = 0.24f;

    public readonly float urgentlyThirstyBorder = 0.12f;

    // public SimpleCurve fallPerTickFromTempCurve = new SimpleCurve
    // {
    // new CurvePoint(30f, 0f),
    // new CurvePoint(130f, 1.5E-05f),
    // };
}