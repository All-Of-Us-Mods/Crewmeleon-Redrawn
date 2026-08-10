using UnityEngine;

namespace Crewmeleon_Redrawn.Components;

/// <summary>
/// The brush settings a stroke was drawn with, quantised for the wire. Travels with the stroke so
/// every client rasterises it identically.
/// </summary>
public readonly struct BrushStamp(Color32 color, byte radius, byte opacity, byte hardness, BrushShape shape)
{
    public Color32 Color => color;
    public byte Radius => radius;
    public byte Opacity => opacity;
    public byte Hardness => hardness;
    public BrushShape Shape => shape;

    public static BrushStamp From(BrushSettings brush) => new(
        brush.Color,
        (byte) brush.Radius,
        (byte) Mathf.RoundToInt(brush.Opacity * 255f),
        (byte) Mathf.RoundToInt(brush.Hardness * 255f),
        brush.Shape);

    /// <summary>Alpha at <paramref name="distance"/> pixels from the stamp centre.</summary>
    public float AlphaAt(float distance) => FalloffAt(distance) * (opacity / 255f);

    /// <summary>
    /// Edge falloff alone, without opacity — separated so a cached kernel stays valid when only
    /// the opacity changes.
    /// </summary>
    public float FalloffAt(float distance) => FalloffFromNormalized(radius <= 0 ? 0f : distance / radius);

    /// <summary>
    /// Falloff for an offset already expressed as 0 at the centre and 1 at the brush edge. Shape
    /// decides how that distance is measured, so square brushes fall off towards their border
    /// rather than towards an inscribed circle.
    /// </summary>
    public float FalloffFromNormalized(float normalized)
    {
        if (radius <= 0) return 1f;

        normalized = Mathf.Clamp01(normalized);

        var hardness01 = hardness / 255f;
        if (normalized <= hardness01) return 1f;

        return 1f - Mathf.InverseLerp(hardness01, 1f, normalized);
    }

    /// <summary>Distance metric for the shape: radial for a circle, Chebyshev for a square.</summary>
    public float NormalizedOffset(int dx, int dy)
    {
        if (radius <= 0) return 0f;

        return shape == BrushShape.Square
            ? Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) / (float) radius
            : Mathf.Sqrt(dx * dx + dy * dy) / radius;
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
