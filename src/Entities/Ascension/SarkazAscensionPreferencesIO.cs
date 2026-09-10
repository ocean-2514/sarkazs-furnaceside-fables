namespace Skz.Ocean.Entities.Ascension;

using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Saves;

/// <summary>Stores only the last level selected in the character-select screen.</summary>
public static class SarkazAscensionPreferencesIO
{
    private const string FileName = "sarkaz_preferences.save";

    private static string FilePath => SaveManager.Instance.GetProfileScopedPath(FileName);

    public static int LoadPreferredLevel()
    {
        if (!FileAccess.FileExists(FilePath)) return 0;

        using FileAccess file = FileAccess.Open(FilePath, FileAccess.ModeFlags.Read);
        if (file == null) return 0;

        Variant parsed = Json.ParseString(file.GetAsText());
        if (parsed.VariantType != Variant.Type.Dictionary) return 0;

        Godot.Collections.Dictionary root = parsed.AsGodotDictionary();
        return root.ContainsKey("preferred_ascension")
            ? System.Math.Clamp(root["preferred_ascension"].AsInt32(),
                SarkazAscensionState.MinLevel, SarkazAscensionState.MaxLevel)
            : 0;
    }

    public static void SavePreferredLevel(int level)
    {
        level = System.Math.Clamp(level,
            SarkazAscensionState.MinLevel, SarkazAscensionState.MaxLevel);
        var root = new Godot.Collections.Dictionary
        {
            { "preferred_ascension", level },
        };

        using FileAccess file = FileAccess.Open(FilePath, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            Log.Warn($"[Sarkaz] Failed to write ascension preferences: {FileAccess.GetOpenError()}");
            return;
        }
        file.StoreString(Json.Stringify(root));
    }
}
