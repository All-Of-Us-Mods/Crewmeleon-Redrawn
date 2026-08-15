using UnityEngine;

namespace CrewmeleonRedrawn.Components;

/// <summary>brush settings baked into a stroke so every client draws it the same</summary>
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

    /// <summary>alpha this far from the stamp centre</summary>
    public float AlphaAt(float distance) => FalloffAt(distance) * (opacity / 255f);

    /// <summary>falloff without opacity so the cached kernel survives an opacity change</summary>
    public float FalloffAt(float distance) => FalloffFromNormalized(radius <= 0 ? 0f : distance / radius);

    /// <summary>falloff for an offset already scaled to 0 at the centre and 1 at the edge</summary>
    public float FalloffFromNormalized(float normalized)
    {
        if (radius <= 0) return 1f;

        normalized = Mathf.Clamp01(normalized);

        var hardness01 = hardness / 255f;
        if (normalized <= hardness01) return 1f;

        return 1f - Mathf.InverseLerp(hardness01, 1f, normalized);
    }

    /// <summary>how much this pixel gets covered given its offset from the centre</summary>
    public float WeightAt(int dx, int dy)
    {
        if (radius <= 0) return 1f;

        var offset = Mathf.Sqrt(dx * dx + dy * dy) / radius;
        return offset > 1f ? 0f : FalloffFromNormalized(offset);
    }
}

/// <summary>
/// struct containing the path along with the brush settings that were used for that stroke.
/// </summary>
public readonly struct PaintStroke(BrushStamp brush, Vector2Int[] points)
{
    public BrushStamp Brush => brush;
    public Vector2Int[] Points => points;
}

public readonly struct StrokeUndo(int x, int y, int width, int height, Color32[] pixels)
{
    public int X => x;
    public int Y => y;
    public int Width => width;
    public int Height => height;
    public Color32[] Pixels => pixels;

    public bool HasPixels => pixels is { Length: > 0 };
}

/// <summary>
/// strokes are chunked to avoid one large packet being sent
/// </summary>
public readonly struct StrokeChunk(uint strokeId, uint chunkIndex, uint chunkCount, BrushStamp brush, Vector2Int[] points)
{
    public uint StrokeId => strokeId;
    public uint ChunkIndex => chunkIndex;
    public uint ChunkCount => chunkCount;
    public bool IsFirst => chunkIndex == 0;
    public BrushStamp Brush => brush;
    public Vector2Int[] Points => points;
}
