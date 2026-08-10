namespace Crewmeleon_Redrawn.Components;

/// <summary>
/// The local player's brush. Lives outside the canvas component so the UI panel can bind to it
/// without caring whether a canvas currently exists.
/// </summary>
public static class BrushStore
{
    public static BrushSettings Local { get; } = new();
}
