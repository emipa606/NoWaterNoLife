using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;

namespace MizuMod
{
    [HarmonyPatch(typeof(Caravan))]
    [HarmonyPatch("GetInspectString")]
    internal class Caravan_GetInspectString
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var found_caravanDaysOfFood = false;
            var foundNum_Pop = 0;
            var insert_index = -1;
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (found_caravanDaysOfFood == false)
                {
                    if (codes[i].opcode != OpCodes.Ldstr ||
                        codes[i].operand.ToString().Contains("CaravanDaysOfFoodRot") ||
                        !codes[i].operand.ToString().Contains("CaravanDaysOfFood"))
                    {
                        continue;
                    }

                    found_caravanDaysOfFood = true;
                    foundNum_Pop = 0;
                }
                else if (foundNum_Pop < 1)
                {
                    if (codes[i].opcode == OpCodes.Pop)
                    {
                        foundNum_Pop++;
                    }
                }
                else
                {
                    insert_index = i;
                    break;
                }
            }

            if (insert_index <= -1)
            {
                return codes.AsEnumerable();
            }

            var new_codes = new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(MizuCaravanUtility),
                        nameof(MizuCaravanUtility.AppendWaterWorthToCaravanInspectString)))
            };

            codes.InsertRange(insert_index + 1, new_codes);

            return codes.AsEnumerable();
        }
    }
}