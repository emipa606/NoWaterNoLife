using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;

namespace MizuMod
{
    public class StatPart_Water : StatPart
    {
        private readonly float factorHealthy = 1f;
        private readonly float factorSlightlyThirsty = 1f;
        private readonly float factorThirsty = 1f;
        private readonly float factorUrgentThirsty = 1f;
        private readonly float factorDehydration = 1f;

        public override void TransformValue(StatRequest req, ref float val)
        {
            // 水分要求を持ったポーンであるかどうかチェック
            if (!req.HasThing)
            {
                return;
            }

            if (!(req.Thing is Pawn pawn) || pawn.needs.Water() == null)
            {
                return;
            }

            val *= WaterMultiplier(pawn.needs.Water().CurCategory);
        }

        public override string ExplanationPart(StatRequest req)
        {
            // 水分要求を持ったポーンであるかどうかチェック
            if (!req.HasThing)
            {
                return null;
            }

            if (!(req.Thing is Pawn pawn) || pawn.needs.Water() == null)
            {
                return null;
            }

            return GetLabel(pawn.needs.Water().CurCategory) + ": x" + WaterMultiplier(pawn.needs.Water().CurCategory).ToStringPercent();
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

    }
}
