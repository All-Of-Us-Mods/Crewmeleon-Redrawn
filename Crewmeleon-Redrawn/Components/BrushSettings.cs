using UnityEngine;

namespace CrewmeleonRedrawn.Components;

/// <summary>local players brush settings, stores HSV, opacity, and size/hardness</summary>
public class BrushSettings
{
    private float hue;
    private float saturation;
    private float value = 1f;
    private float opacity = 1f;
    private float hardness = 1f;
    private int radius = 3;

    /// <summary>bumped on every change so the ui knows when to redraw</summary>
    public int Version { get; private set; }


    public float Hue
    {
        get => hue;
        set { hue = Mathf.Repeat(value, 1f); Version++; }
    }

    public float Saturation
    {
        get => saturation;
        set { saturation = Mathf.Clamp01(value); Version++; }
    }

    public float Value
    {
        get => value;
        set { this.value = Mathf.Clamp01(value); Version++; }
    }

    /// <summary>opacity </summary>
    public float Opacity
    {
        get => opacity;
        set { opacity = Mathf.Clamp01(value); Version++; }
    }

    /// <summary>hardness fades brush out from the center</summary>
    public float Hardness
    {
        get => hardness;
        set { hardness = Mathf.Clamp01(value); Version++; }
    }

    public int Radius
    {
        get => radius;
        set { radius = Mathf.Clamp(value, MinRadius, MaxRadius); Version++; }
    }

    public const int MinRadius = 1;
    public const int MaxRadius = 15;

    /// <summary>colour without opacity</summary>
    public Color Color => Color.HSVToRGB(hue, saturation, value);

    public void SetFromColor(Color color)
    {
        Color.RGBToHSV(color, out hue, out saturation, out value);
    }

    /// <summary>flat Opacity inside the hard core then fades to nothing at the rim</summary>
    public float AlphaAt(float distance)
    {
        if (radius <= 0) return opacity;

        var normalized = Mathf.Clamp01(distance / radius);
        if (normalized <= hardness) return opacity;

        // hardness of exactly 1 would divide by zero
        var falloff = 1f - Mathf.InverseLerp(hardness, 1f, normalized);
        return opacity * falloff;
    }
}
