using UnityEngine;

namespace Crewmeleon_Redrawn.Components;

/// <summary>
/// The local player's brush. Stored as HSV so the picker can move hue independently of a
/// desaturated or dark colour, which round-tripping through RGB would lose.
/// </summary>
public class BrushSettings
{
    private float hue;
    private float saturation;
    private float value = 1f;
    private float opacity = 1f;
    private float hardness = 1f;
    private int radius = 3;

    public float Hue
    {
        get => hue;
        set => hue = Mathf.Repeat(value, 1f);
    }

    public float Saturation
    {
        get => saturation;
        set => saturation = Mathf.Clamp01(value);
    }

    public float Value
    {
        get => value;
        set => this.value = Mathf.Clamp01(value);
    }

    /// <summary>Alpha applied to each painted pixel, letting strokes build up.</summary>
    public float Opacity
    {
        get => opacity;
        set => opacity = Mathf.Clamp01(value);
    }

    /// <summary>1 is a hard edge; lower values fade the brush out towards its rim.</summary>
    public float Hardness
    {
        get => hardness;
        set => hardness = Mathf.Clamp01(value);
    }

    public int Radius
    {
        get => radius;
        set => radius = Mathf.Clamp(value, MinRadius, MaxRadius);
    }

    public const int MinRadius = 1;
    public const int MaxRadius = 15;

    /// <summary>Fully opaque brush colour, ignoring <see cref="Opacity"/>.</summary>
    public Color Color => Color.HSVToRGB(hue, saturation, value);

    public void SetFromColor(Color color)
    {
        Color.RGBToHSV(color, out hue, out saturation, out value);
    }

    /// <summary>
    /// Alpha for a pixel at <paramref name="distance"/> from the brush centre. Inside the hard
    /// core it is flat <see cref="Opacity"/>, then falls to zero at the rim.
    /// </summary>
    public float AlphaAt(float distance)
    {
        if (radius <= 0) return opacity;

        var normalized = Mathf.Clamp01(distance / radius);
        if (normalized <= hardness) return opacity;

        // avoid a divide by zero when hardness is exactly 1
        var falloff = 1f - Mathf.InverseLerp(hardness, 1f, normalized);
        return opacity * falloff;
    }
}
