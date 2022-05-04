using System.Reflection;
using HarmonyLib;
using Verse;

namespace MizuMod;

[StaticConstructorOnStartup]
internal class Main
{
    static Main()
    {
        var harmony = new Harmony("com.himesama.mizumod");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}

// Uncommented for now, as I have no clue how to merge that code in the B19 version of CaravanPansNeedsUtility
// キャラバン移動中に水分要求を補充する行動を追加

// 所持品欄の水アイテムに「水を飲む」ボタンを追加

// キャラバンの荷物選択時、右上に現在の水分総量を表示させる処理を追加

// 水アイテム不足で出発しようとしたとき、警告ダイアログを出す
// 実際の処理は、食料不足の警告ダイアログに便乗する形
// This is broken in Rimworld 1.1. Quick-fixing to remove it temporarily in 1.8.1 with the goal of fixing it in 1.8.2
/*
[HarmonyPatch(typeof(Dialog_FormCaravan))]
[HarmonyPatch("DoBottomButtons")]
class Dialog_FormCaravan_DoBottomButtons
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        int key_index = -1;
        int insert_index = -1;
        var codes = new List<CodeInstruction>(instructions);

        // 処理を挿入すべき場所の近くにあった文字列のnullチェック処理を手掛かりにする(かなり強引なやり方)
        for (int i = 0; i < codes.Count; i++)
        {
            if (key_index < 0 && codes[i].opcode == OpCodes.Ldstr && codes[i].operand.ToString() == "CaravanFoodWillRotSoonWarningDialog")
            {
                key_index = i;
            }
            if (key_index >= 0 && insert_index < 0 && codes[i].opcode == OpCodes.Ldarg_0)
            {
                insert_index = i + 1;
            }
            //if (found_ldnull < 2 && codes[i].opcode == OpCodes.Ldnull)
            //{
            //    replace_index = i;
            //    found_ldnull++;
            //}
            //if (found_ldnull >= 2 && codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("NullOrEmpty"))
            //{
            //    insert_index = i;
            //}
        }

        if (key_index > -1 && insert_index > -1)
        {
            //List<CodeInstruction> replace_codes = new List<CodeInstruction>();
            //replace_codes.Add(new CodeInstruction(OpCodes.Ldstr, string.Empty));
            //codes[replace_index].opcode = OpCodes.Nop;
            //codes.InsertRange(replace_index + 1, replace_codes);
            //insert_index += replace_codes.Count;

            List<CodeInstruction> insert_codes = new List<CodeInstruction>();
            //codes[insert_index - 1].opcode = OpCodes.Nop;
            //insert_codes.Add(new CodeInstruction(OpCodes.Ldstr, "{0}{1}"));

            //insert_codes.Add(new CodeInstruction(OpCodes.Ldc_I4_2));
            //insert_codes.Add(new CodeInstruction(OpCodes.Newarr, typeof(object)));

            //insert_codes.Add(new CodeInstruction(OpCodes.Dup));
            //insert_codes.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
            //insert_codes.Add(new CodeInstruction(OpCodes.Ldloc_1));
            //insert_codes.Add(new CodeInstruction(OpCodes.Stelem_Ref));

            //insert_codes.Add(new CodeInstruction(OpCodes.Dup));
            //insert_codes.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
            //insert_codes.Add(new CodeInstruction(OpCodes.Ldstr, "\n\n uhawww okwww"));
            //insert_codes.Add(new CodeInstruction(OpCodes.Stelem_Ref));

            //insert_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(string), nameof(string.Format), new Type[] { typeof(string), typeof(object[]) })));
            //insert_codes.Add(new CodeInstruction(OpCodes.Stloc_1));

            insert_codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
            insert_codes.Add(new CodeInstruction(OpCodes.Ldloc_S, 4));
            insert_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MizuCaravanUtility), nameof(MizuCaravanUtility.AddWaterWarningString), new Type[] { typeof(Dialog_FormCaravan), typeof(List<string>) })));
            //insert_codes.Add(new CodeInstruction(OpCodes.Ldarg_0));

            codes.InsertRange(insert_index, insert_codes);
        }
        return codes.AsEnumerable();
    }
}
*/

// キャラバン編成画面で積荷の内容が変化したときに、水の総量再計算フラグを立てる処理を追加

// 惑星画面でキャラバンを選んだ時に左下に出てくる情報ウィンドウに積荷の水分量表示を追加

// ポーンの所持品生成処理(トレーダーがやってくるとき等)に水アイテムを持たせる処理を追加

// 水アイテムが囚人部屋に置いてあるとき、囚人用として扱う処理を追加

// 食べ物に水分設定がある場合、食べた後に水分も回復する処理を追加

// 包囲攻撃の仕送りで食料を送るときに一緒に水も送るようにする

// 輸送ポッド積荷選択時、右上に現在の水分総量を表示させる処理を追加

// よくわからないけどなぜか追加できないのでコメントアウト

// 輸送ポッド積荷選択画面で積荷の内容が変化したときに、水の総量再計算フラグを立てる処理を追加

// 食事要求がないポーンの水分要求を非表示にする

// キャラバンのトレード時、上に現在の水分総量を表示させる処理を追加

// キャラバントレード画面で荷物の内容が変化したときに、水の総量再計算フラグを立てる処理を追加

// 水やり状態を肥沃度に反映させる
// [HarmonyPatch(typeof(FertilityGrid))]
// [HarmonyPatch("CalculateFertilityAt")]
// class FertilityGrid_CalculateFertilityAt
// {
// static void Postfix(FertilityGrid __instance, ref IntVec3 loc, ref float __result)
// {
// var map = Traverse.Create(__instance).Field("map").GetValue<Map>();
// int wateringRemainTicks = map.GetComponent<MapComponent_Watering>().Get(map.cellIndices.CellToIndex(loc));
// if (wateringRemainTicks > 0)
// {
// // 水やりされている
// __result *= MizuModBody.Settings.FertilityFactorInWatering;
// }
// else
// {
// // 水やりされていない
// __result *= MizuModBody.Settings.FertilityFactorInNotWatering;
// }
// }
// }

// 水やり状態を成長速度に反映させる