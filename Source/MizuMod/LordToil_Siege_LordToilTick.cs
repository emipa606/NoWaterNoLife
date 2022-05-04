using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace MizuMod;

[HarmonyPatch(typeof(LordToil_Siege))]
[HarmonyPatch("LordToilTick")]
internal class LordToil_Siege_LordToilTick
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var insert_index = -1;
        var codes = new List<CodeInstruction>(instructions);

        for (var i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("DropSupplies"))
            {
                insert_index = i + 1;
            }
        }

        if (insert_index <= -1)
        {
            return codes.AsEnumerable();
        }

        var insert_codes = new List<CodeInstruction>
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Ldsfld,
                AccessTools.Field(typeof(MizuDef), nameof(MizuDef.Thing_ClearWater))),
            new CodeInstruction(OpCodes.Ldc_I4_S, 20),
            new CodeInstruction(OpCodes.Call,
                AccessTools.Method(typeof(LordToil_Siege), "DropSupplies", new[] { typeof(ThingDef), typeof(int) }))
        };

        codes.InsertRange(insert_index, insert_codes);

        return codes.AsEnumerable();
    }
}