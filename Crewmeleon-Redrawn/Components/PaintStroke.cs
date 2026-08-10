using UnityEngine;

namespace Crewmeleon_Redrawn.Components;

/// <summary>
/// The brush settings a stroke was drawn with, quantised for the wire. Travels with the stroke so
/// every client rasterises it identically.
/// </summary>
public readonly struct BrushStamp(Color32 color, byte radius, byte opacity, byte hardness, BrushShape shape, BrushMask? mask = null, bool mirrored = false)
{
    public Color32 Color => color;
    public byte Radius => radius;
    public byte Opacity => opacity;
    public byte Hardness => hardness;
    public BrushShape Shape => shape;
    public BrushMask? Mask => mask;

    /// <summary>
    /// The canvas is drawn mirrored when the player faces left. The position is already mapped
    /// into texture space, but an asymmetric tip has to be mirrored too or it renders reversed.
    /// Baked into the stroke so replay doesn't depend on which way the painter happens to face.
    /// </summary>
    public bool Mirrored => mirrored;

    public static BrushStamp From(BrushSettings brush, bool mirrored = false) => new(
        brush.Color,
        (byte) brush.Radius,
        (byte) Mathf.RoundToInt(brush.Opacity * 255f),
        (byte) Mathf.RoundToInt(brush.Hardness * 255f),
        brush.Shape,
        brush.Mask,
        mirrored);

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

    /// <summary>
    /// Coverage weight for a pixel offset from the stamp centre. A custom tip supplies its own
    /// shape and softness, so hardness doesn't apply to it.
    /// </summary>
    public float WeightAt(int dx, int dy)
    {
        if (radius <= 0) return 1f;

        if (shape == BrushShape.Custom)
        {
            var nx = (mirrored ? -dx : dx) / (float) radius;
            return mask?.Sample(nx, dy / (float) radius) ?? 0f;
        }

        var offset = shape == BrushShape.Square
            ? Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) / (float) radius
            : Mathf.Sqrt(dx * dx + dy * dy) / radius;

        return offset > 1f ? 0f : FalloffFromNormalized(offset);
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

/// <summary>
/// One packet of a stroke. A full path can run past the message size limit, so the brush goes out
/// in the opening chunk and the points follow; the receiver composites when <see cref="IsFinal"/>
/// arrives.
/// </summary>
public readonly struct StrokeChunk(bool isFirst, bool isFinal, BrushStamp brush, Vector2Int[] points)
{
    public bool IsFirst => isFirst;
    public bool IsFinal => isFinal;
    public BrushStamp Brush => brush;
    public Vector2Int[] Points => points;
}
