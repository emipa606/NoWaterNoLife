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