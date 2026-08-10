using UnityEngine;

namespace Crewmeleon_Redrawn.Components;

/// <summary>
/// The brush settings a stroke was drawn with, quantised for the wire. Travels with the stroke so
/// every client rasterises it identically.
/// </summary>
public readonly struct BrushStamp(Color32 color, byte radius, byte opacity, byte hardness)
{
    public Color32 Color => color;
    public byte Radius => radius;
    public byte Opacity => opacity;
    public byte Hardness => hardness;

    public static BrushStamp From(BrushSettings brush) => new(
        brush.Color,
        (byte) brush.Radius,
        (byte) Mathf.RoundToInt(brush.Opacity * 255f),
        (byte) Mathf.RoundToInt(brush.Hardness * 255f));

    /// <summary>Alpha at <paramref name="distance"/> pixels from the stamp centre.</summary>
    public float AlphaAt(float distance) => FalloffAt(distance) * (opacity / 255f);

    /// <summary>
    /// Edge falloff alone, without opacity — separated so a cached kernel stays valid when only
    /// the opacity changes.
    /// </summary>
    public float FalloffAt(float distance)
    {
        if (radius <= 0) return 1f;

        var hardness01 = hardness / 255f;
        var normalized = Mathf.Clamp01(distance / radius);
        if (normalized <= hardness01) return 1f;

        return 1f - Mathf.InverseLerp(hardness01, 1f, normalized);
    }
}

/// <summary>
/// One continuous drag: the brush used, plus the path it followed. Storing the path rather than
/// the affected pixels keeps strokes small on the wire and makes undo a matter of dropping the
/// last entry and replaying the rest.
/// </summary>
public readonly struct PaintStroke(BrushStamp brush, Vector2Int[] points)
{
    public BrushStamp Brush => brush;
    public Vector2Int[] Points => points;
}
