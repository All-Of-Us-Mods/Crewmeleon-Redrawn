using BepInEx.Configuration;

namespace Crewmeleon_Redrawn.Components;

/// <summary>
/// The brushes shown in the panel: two built-ins that always exist, plus whatever the player has
/// saved. Saved brushes persist through the plugin config.
/// </summary>
public static class BrushLibrary
{
    private static readonly BrushPreset[] BuiltIn =
    [
        new("Circle", BrushShape.Circle, 3, 1f, 1f),
        new("Square", BrushShape.Square, 3, 1f, 1f),
    ];

    private static readonly List<BrushPreset> Saved = [];

    private static ConfigEntry<string>? storage;

    public static IReadOnlyList<BrushPreset> All => BuiltIn.Concat(Offered()).ToList();

    private static IEnumerable<BrushPreset> Offered()
    {
#if CUSTOM_BRUSHES
        return Saved;
#else
        // hidden rather than dropped: they stay in config, so enabling authoring again brings
        // the player's brushes back instead of silently destroying them
        return Saved.Where(preset => preset.Shape != BrushShape.Custom);
#endif
    }

    public static void Load(ConfigFile config)
    {
        storage = config.Bind("Brushes", "Saved", string.Empty, "Player-saved brush presets.");

        Saved.Clear();
        foreach (var entry in storage.Value.Split(';'))
        {
            if (string.IsNullOrWhiteSpace(entry)) continue;

            var preset = BrushPreset.Deserialize(entry);
            if (preset != null) Saved.Add(preset);
        }
    }

    public static void SaveCurrent(BrushSettings brush)
    {
        Add(BrushPreset.From(NextName(), brush));
    }

    public static BrushPreset AddCustom(BrushMask mask)
    {
        var preset = new BrushPreset(NextName(), BrushShape.Custom, 3, 1f, 1f, mask);
        Add(preset);
        return preset;
    }

    private static void Add(BrushPreset preset)
    {
        Saved.Add(preset);
        Persist();
    }

    public static void Remove(BrushPreset preset)
    {
        if (!Saved.Remove(preset)) return;
        Persist();
    }

    public static bool CanRemove(BrushPreset preset) => Saved.Contains(preset);

    private static string NextName()
    {
        var index = Saved.Count + 1;
        while (Saved.Any(p => p.Name == $"Brush {index}")) index++;
        return $"Brush {index}";
    }

    private static void Persist()
    {
        if (storage == null) return;
        storage.Value = string.Join(";", Saved.Select(p => p.Serialize()));
    }
}
