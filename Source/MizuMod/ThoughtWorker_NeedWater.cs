﻿using System;
using RimWorld;
using Verse;

namespace MizuMod;

public class ThoughtWorker_NeedWater : ThoughtWorker
{
    protected override ThoughtState CurrentStateInternal(Pawn p)
    {
        if (p.needs.Water() == null)
        {
            return ThoughtState.Inactive;
        }

        switch (p.needs.Water().CurCategory)
        {
            case ThirstCategory.Healthy:
                return ThoughtState.Inactive;
            case ThirstCategory.SlightlyThirsty:
                return ThoughtState.ActiveAtStage(0);
            case ThirstCategory.Thirsty:
                return ThoughtState.ActiveAtStage(1);
            case ThirstCategory.UrgentlyThirsty:
                return ThoughtState.ActiveAtStage(2);
            case ThirstCategory.Dehydration:
            {
                var firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(MizuDef.Hediff_Dehydration);
                var num = firstHediffOfDef?.CurStageIndex ?? 0;

                // 脱水症状の1段階目=喉の渇き心情の4段階目 => +3する
                return ThoughtState.ActiveAtStage(3 + num);
            }

            default:
                throw new NotImplementedException();
        }
    }
}