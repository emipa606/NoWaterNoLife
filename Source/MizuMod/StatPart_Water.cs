using System;
using RimWorld;
using Verse;

namespace MizuMod;

public class StatPart_Water : StatPart
{
    private readonly float factorDehydration = 1f;

    private readonly float factorHealthy = 1f;

    private readonly float factorSlightlyThirsty = 1f;

    private readonly float factorThirsty = 1f;

    private readonly float factorUrgentThirsty = 1f;

    public override string ExplanationPart(StatRequest req)
    {
        // 水分要求を持ったポーンであるかどうかチェック
        if (!req.HasThing)
        {
            return null;
        }

        if (req.Thing is not Pawn pawn || pawn.needs.Water() == null)
        {
            return null;
        }

        return
            $"{GetLabel(pawn.needs.Water().CurCategory)}: x{WaterMultiplier(pawn.needs.Water().CurCategory).ToStringPercent()}";
    }

    public override void TransformValue(StatRequest req, ref float val)
    {
        // 水分要求を持ったポーンであるかどうかチェック
        if (!req.HasThing)
        {
            return;
        }

        if (req.Thing is not Pawn pawn || pawn.needs.Water() == null)
        {
            return;
        }

        val *= WaterMultiplier(pawn.needs.Water().CurCategory);
    }

    private string GetLabel(ThirstCategory thirst)
    {
        switch (thirst)
        {
            case ThirstCategory.Healthy:
                return "MizuThirstLevel_Healthy".Translate();
            case ThirstCategory.SlightlyThirsty:
                return "MizuThirstLevel_SlightlyThirsty".Translate();
            case ThirstCategory.Thirsty:
                return "MizuThirstLevel_Thirsty".Translate();
            case ThirstCategory.UrgentlyThirsty:
                return "MizuThirstLevel_UrgentlyThirsty".Translate();
            case ThirstCategory.Dehydration:
                return "MizuThirstLevel_Dehydration".Translate();
            default:
                throw new InvalidOperationException();
        }
    }

    private float WaterMultiplier(ThirstCategory thirst)
    {
        switch (thirst)
        {
            case ThirstCategory.Healthy:
                return factorHealthy;
            case ThirstCategory.SlightlyThirsty:
                return factorSlightlyThirsty;
            case ThirstCategory.Thirsty:
                return factorThirsty;
            case ThirstCategory.UrgentlyThirsty:
                return factorUrgentThirsty;
            case ThirstCategory.Dehydration:
                return factorDehydration;
            default:
                throw new InvalidOperationException();
        }
    }
}