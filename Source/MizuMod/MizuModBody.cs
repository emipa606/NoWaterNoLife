using UnityEngine;
using Verse;

namespace MizuMod;

public class MizuModBody : Mod
{
    public static Settings Settings;

    public MizuModBody(ModContentPack content) : base(content)
    {
        Settings = GetSettings<Settings>();
    }

    public override string SettingsCategory()
    {
        return MizuStrings.ModTitle;
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        Settings.DoSettingsWindowContents(inRect);
    }
}