﻿using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace MizuMod;

[StaticConstructorOnStartup]
public class JobGiver_DrawWaterByPrisoner : ThinkNode_JobGiver
{
    private const int SearchDrawerIntervalTick = 180;

    private static readonly List<Thing> drawerList = [];

    public override float GetPriority(Pawn pawn)
    {
        return 1f;
    }

    protected override Job TryGiveJob(Pawn pawn)
    {
        drawerList.Clear();

        var need_water = pawn.needs.Water();
        if (need_water == null)
        {
            return null;
        }

        // 最後に水汲み設備を探してから少し経つまで次の探索はしない
        if (need_water.lastSearchWaterTick + SearchDrawerIntervalTick > Find.TickManager.TicksGame)
        {
            return null;
        }

        need_water.lastSearchWaterTick = Find.TickManager.TicksGame;

        // 手が使えなければ水汲みはできない
        if (!pawn.CanManipulate())
        {
            return null;
        }

        // 部屋を取得
        var myRoom = pawn.Position.GetRoom(pawn.Map);
        if (myRoom == null)
        {
            return null;
        }

        // 部屋の中にある水汲み設備を探す
        foreach (var t in pawn.Map.listerThings.AllThings)
        {
            var isDrawer = false;

            // 同じ部屋になければダメ
            if (t.Position.GetRoom(t.Map) != myRoom)
            {
                continue;
            }

            // レシピを持っていないものはダメ
            if (t.def.recipes == null)
            {
                continue;
            }

            // 水汲みレシピを持っているかチェック
            foreach (var recipe in t.def.recipes)
            {
                var ext = recipe.GetModExtension<DefExtension_WaterRecipe>();
                if (ext == null)
                {
                    continue;
                }

                if (ext.recipeType != DefExtension_WaterRecipe.RecipeType.DrawFromTerrain
                    && ext.recipeType != DefExtension_WaterRecipe.RecipeType.DrawFromWaterNet
                    && ext.recipeType != DefExtension_WaterRecipe.RecipeType.DrawFromWaterPool)
                {
                    continue;
                }

                isDrawer = true;
                break;
            }

            if (isDrawer)
            {
                drawerList.Add(t);
            }
        }

        Thing bestDrawer = null;
        var bestWaterType = WaterType.SeaWater;

        // 部屋の中の水汲み設備の中で最良の条件の物を探す
        foreach (var drawer in drawerList)
        {
            // 水汲みが出来るものは水を飲むことも出来る
            if (drawer is not IBuilding_DrinkWater drinkWaterBuilding)
            {
                continue;
            }

            // 作業場所をもっているならそこ、そうでないなら隣接セル
            var peMode = drawer.def.hasInteractionCell ? PathEndMode.InteractionCell : PathEndMode.Touch;

            // 予約と到達が出来ないものはダメ
            if (!pawn.CanReserveAndReach(drawer, peMode, Danger.Deadly))
            {
                continue;
            }

            // 動作していないものはダメ
            if (!drinkWaterBuilding.IsActivated)
            {
                continue;
            }

            // 汲むことが出来ないものはダメ
            if (!drinkWaterBuilding.CanDrawFor(pawn))
            {
                continue;
            }

            // 水の種類が飲めないタイプの物はダメ
            if (drinkWaterBuilding.WaterType is WaterType.Undefined or WaterType.NoWater)
            {
                continue;
            }

            if (bestWaterType > drinkWaterBuilding.WaterType)
            {
                continue;
            }

            // 水質が悪くなければ、更新
            bestDrawer = drawer;
            bestWaterType = drinkWaterBuilding.WaterType;
        }

        // 水汲み設備が見つからなかった
        return bestDrawer == null ? null : new Job(MizuDef.Job_DrawWaterByPrisoner, bestDrawer);
    }
}