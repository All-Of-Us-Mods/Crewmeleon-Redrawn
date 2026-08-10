using System.Globalization;
using UnityEngine;

namespace Crewmeleon_Redrawn.Components;

/// <summary>
/// A saved brush. Deliberately carries no colour — picking a brush shouldn't discard the colour
/// you just chose.
/// </summary>
public sealed class BrushPreset(string name, BrushShape shape, int radius, float opacity, float hardness)
{
    public string Name { get; } = name;
    public BrushShape Shape { get; } = shape;
    public int Radius { get; } = radius;
    public float Opacity { get; } = opacity;
    public float Hardness { get; } = hardness;

    public static BrushPreset From(string name, BrushSettings brush) =>
        new(name, brush.Shape, brush.Radius, brush.Opacity, brush.Hardness);

    public void ApplyTo(BrushSettings brush)
    {
        brush.Shape = Shape;
        brush.Radius = Radius;
        brush.Opacity = Opacity;
        brush.Hardness = Hardness;
    }

    public bool Matches(BrushSettings brush) =>
        brush.Shape == Shape
        && brush.Radius == Radius
        && Mathf.Approximately(brush.Opacity, Opacity)
        && Mathf.Approximately(brush.Hardness, Hardness);

    public string Serialize()
    {
        var safeName = Name.Replace('|', ' ').Replace(';', ' ');
        return string.Join("|", safeName, (byte) Shape, Radius,
            Opacity.ToString("R", CultureInfo.InvariantCulture),
            Hardness.ToString("R", CultureInfo.InvariantCulture));
    }

    public static BrushPreset? Deserialize(string value)
    {
        var parts = value.Split('|');
        if (parts.Length != 5) return null;

        if (!byte.TryParse(parts[1], out var shape)) return null;
        if (!int.TryParse(parts[2], out var radius)) return null;
        if (!float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var opacity)) return null;
        if (!float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out var hardness)) return null;

        return new BrushPreset(parts[0], (BrushShape) shape, radius, opacity, hardness);
    }
}
