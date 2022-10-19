using Mlie;
using UnityEngine;
using Verse;

namespace MizuMod;

public class MizuModBody : Mod
{
    public static Settings Settings;
    public static string currentVersion;

    public MizuModBody(ModContentPack content) : base(content)
    {
        Settings = GetSettings<Settings>();
        currentVersion =
            VersionFromManifest.GetVersionFromModMetaData(ModLister.GetActiveModWithIdentifier("Mlie.NoWaterNoLife"));
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