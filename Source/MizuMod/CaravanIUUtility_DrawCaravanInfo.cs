using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;

namespace MizuMod
{
    [HarmonyPatch(typeof(CaravanUIUtility))]
    [HarmonyPatch("DrawCaravanInfo")]
    internal class CaravanIUUtility_DrawCaravanInfo
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var insert_index = -1;
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                // if (codes[i].operand != null)
                // {
                // Log.Message(string.Format("{0}, {1}, {2}", codes[i].opcode.ToString(), codes[i].operand.GetType().ToString(), codes[i].operand.ToString()));
                // }
                // else
                // {
                // Log.Message(string.Format("{0}", codes[i].opcode.ToString()));
                // }

                // 食料表示処理のコード位置を頼りに挿入すべき場所を探す
                if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("DrawExtraInfo"))
                {
                    /* DrawExtraInfo gets called with two arguments that are push onto the stack first, therefore we want to insert after three opcodes earlier */
                    insert_index = i - 3;

                    // Log.Message("type  = " + codes[i].operand.GetType().ToString());
                    // Log.Message("val   = " + codes[i].operand.ToString());
                    // Log.Message("count = " + codes[i].labels.Count.ToString());
                    // for (int j = 0; j < codes[i].labels.Count; j++)
                    // {
                    // Log.Message(string.Format("label[{0}] = {1}", j, codes[i].labels[j].ToString()));
                    // }
                }
            }

            if (insert_index <= -1)
            {
                return codes.AsEnumerable();
            }

            var new_codes = new List<CodeInstruction> { new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(CaravanUIUtility), "tmpInfo")), new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MizuCaravanUtility), nameof(MizuCaravanUtility.DrawDaysWorthOfWater))) };

            codes.InsertRange(insert_index + 1, new_codes);

            return codes.AsEnumerable();
        }
    }
}