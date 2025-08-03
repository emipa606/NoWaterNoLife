using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MizuMod.HarmonyPatches;

[HarmonyPatch(typeof(Caravan_ForageTracker), "Forage")]
internal class Caravan_ForageTracker_Forage
{
    private static void Postfix(Caravan ___caravan)
    {
        // キャラバンのポーンの水分要求の処理
        foreach (var pawn in ___caravan.pawns)
        {
            var need_water = pawn.needs?.Water();
            if (need_water == null)
            {
                continue;
            }

            // 喉が渇いてない場合は飲まない
            if (need_water.CurCategory <= ThirstCategory.Healthy)
            {
                continue;
            }

            // タイルが0以上(?)、死んでない、ローカルではなく惑星マップ上にいる(キャラバンしてる)、そのポーンが地形から水を飲める(心情がある/ない、脱水症状まで進んでいる/いない、など)
            if (pawn.Tile >= 0 && !pawn.Dead && pawn.IsWorldPawn() && pawn.CanDrinkFromTerrain())
            {
                var drankTerrainType = ___caravan.GetWaterTerrainType();

                // 水を飲めない場所
                if (drankTerrainType == WaterTerrainType.NoWater)
                {
                    continue;
                }

                // 地形から水を飲む
                need_water.CurLevel = 1.0f;

                if (drankTerrainType == WaterTerrainType.SeaWater)
                {
                    // 海水の場合の健康状態悪化
                    pawn.health.AddHediff(HediffMaker.MakeHediff(MizuDef.Hediff_DrankSeaWater, pawn));
                }

                // 心情要求がなければここで終了
                if (pawn.needs.mood == null)
                {
                    continue;
                }

                // 直接水を飲んだ心情付加
                pawn.needs.mood.thoughts.memories.TryGainMemory(
                    pawn.CanManipulate()
                        ? MizuDef.Thought_DrankScoopedWater
                        : MizuDef.Thought_SippedWaterLikeBeast);

                // キャラバンのいる地形に応じた心情を付加
                var thoughtDef = MizuUtility.GetThoughtDefFromTerrainType(drankTerrainType);
                if (thoughtDef != null)
                {
                    // 水の種類による心情
                    pawn.needs.mood.thoughts.memories.TryGainMemory(thoughtDef);
                }

                continue;
            }

            // 水アイテムを探す

            // アイテムが見つからない
            if (!MizuCaravanUtility.TryGetBestWater(___caravan, pawn, out var waterThing, out var inventoryPawn))
            {
                continue;
            }

            // アイテムに応じた水分を摂取＆心情変化＆健康変化
            var numWater = MizuUtility.GetWater(pawn, waterThing, need_water.WaterWanted, false);
            need_water.CurLevel += numWater;
            pawn.records.AddTo(MizuDef.Record_WaterDrank, numWater);

            // 水アイテムが消滅していない場合(スタックの一部だけ消費した場合等)はここで終了
            if (!waterThing.Destroyed)
            {
                continue;
            }

            if (inventoryPawn != null)
            {
                // 誰かの所持品にあった水スタックを飲みきったのであれば、所持品欄から除去
                inventoryPawn.inventory.innerContainer.Remove(waterThing);

                // 移動不可状態を一旦リセット(して再計算させる？)
                ___caravan.RecacheInventory();

                // 水の残量再計算フラグON
                MizuCaravanUtility.daysWorthOfWaterDirty = true;
            }

            if (!MizuCaravanUtility.TryGetBestWater(___caravan, pawn, out waterThing, out inventoryPawn))
            {
                // 飲んだことにより水がなくなったら警告を出す
                Messages.Message(
                    string.Format(
                        MizuStrings.MessageCaravanRunOutOfWater.Translate(),
                        ___caravan.LabelCap,
                        pawn.Label),
                    ___caravan,
                    MessageTypeDefOf.ThreatBig);
            }
        }
    }
}