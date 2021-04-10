using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace MizuMod
{
    [HarmonyPatch(typeof(Thing))]
    [HarmonyPatch("Ingested")]
    internal class Thing_Ingested
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var insert_index = -1;
            var codes = new List<CodeInstruction>(instructions);

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Callvirt || !codes[i].operand.ToString().Contains("PostIngested"))
                {
                    continue;
                }

                insert_index = i - 1;
                break;
            }

            if (insert_index <= -1)
            {
                return codes.AsEnumerable();
            }

            var insert_codes = new List<CodeInstruction>();
            codes[insert_index - 1].opcode = OpCodes.Nop;

            insert_codes.Add(new CodeInstruction(OpCodes.Ldarg_1));
            insert_codes.Add(new CodeInstruction(OpCodes.Ldarg_0));
            insert_codes.Add(new CodeInstruction(OpCodes.Ldloc_0));
            insert_codes.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(MizuUtility), nameof(MizuUtility.PrePostIngested), new[] { typeof(Pawn), typeof(Thing), typeof(int) })));

            insert_codes.Add(new CodeInstruction(OpCodes.Ldarg_0));

            codes.InsertRange(insert_index, insert_codes);

            return codes.AsEnumerable();
        }
    }
}