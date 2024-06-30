using System.Reflection;
using HarmonyLib;
using Verse;

namespace MizuMod;

[StaticConstructorOnStartup]
internal class Main
{
    static Main()
    {
        new Harmony("com.himesama.mizumod").PatchAll(Assembly.GetExecutingAssembly());
    }
}