namespace Crewmeleon_Redrawn.Components;

/// <summary>lives outside the canvas so the panel can bind to it whether or not one exists</summary>
public static class BrushStore
{
    public static BrushSettings Local { get; } = new();
}
