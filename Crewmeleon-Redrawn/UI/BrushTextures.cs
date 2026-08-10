using Crewmeleon_Redrawn.Components;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace Crewmeleon_Redrawn.UI;

public static class BrushTextures
{
    public const int WheelSize = 256;
    public const float WheelRingWidth = 7f;

    /// <summary>
    /// Fraction of the wheel's radius that actually carries colour — the remainder is the baked
    /// white outline. Saturation maps across this, not the full radius.
    /// </summary>
    public static float WheelColorFraction =>
        (WheelSize / 2f - 1f - WheelRingWidth) / (WheelSize / 2f - 1f);

    public const int StripWidth = 256;
    public const int StripHeight = 16;
    public const int PreviewSize = 72;

    private const int CheckerSquare = 6;

    private static Texture2D? colorWheel;
    private static Texture2D? valueStrip;
    private static Texture2D? brushPreview;

#if BRUSHES
    private static readonly Dictionary<BrushPreset, Texture2D> PresetPreviews = [];
#endif

    private static Vector3 lastStripState = new(-1, -1, -1);
    private static Vector4 lastPreviewState = new(-1, -1, -1, -1);
    private static Color lastPreviewColor = new(-1, -1, -1);

    public static Texture2D ColorWheel => colorWheel ??= CreateColorWheel();

    /// <summary>
    /// Black through to the brush's fully lit colour, with the current value marked. Regenerated
    /// only when the colour it depends on actually moves.
    /// </summary>
    public static Texture2D ValueStrip(BrushSettings brush)
    {
        valueStrip ??= NewTexture(StripWidth, StripHeight);

        var state = new Vector3(brush.Hue, brush.Saturation, brush.Value);
        if (state == lastStripState) return valueStrip;
        lastStripState = state;

        var full = Color.HSVToRGB(brush.Hue, brush.Saturation, 1f);

        // inset the ends so the marker isn't clipped by the strip's rounded corners at 0% or 100%
        var markerX = Mathf.RoundToInt(Mathf.Lerp(3f, StripWidth - 4f, brush.Value));

        var pixels = new Color[StripWidth * StripHeight];
        for (var x = 0; x < StripWidth; x++)
        {
            var column = Color.Lerp(Color.black, full, x / (float) (StripWidth - 1));

            var distance = Mathf.Abs(x - markerX);
            if (distance <= 2) column = distance <= 1 ? Color.white : Color.black;

            for (var y = 0; y < StripHeight; y++) pixels[y * StripWidth + x] = column;
        }

        Upload(valueStrip, pixels);
        return valueStrip;
    }

    /// <summary>
    /// The brush as it will actually paint — size, hardness falloff and opacity, over a
    /// checkerboard so partial alpha is visible.
    /// </summary>
    public static Texture2D BrushPreview(BrushSettings brush)
    {
        brushPreview ??= NewTexture(PreviewSize, PreviewSize);

        var state = new Vector4(brush.Radius, brush.Hardness, brush.Opacity, 0);
        if (state == lastPreviewState && brush.Color == lastPreviewColor) return brushPreview;
        lastPreviewState = state;
        lastPreviewColor = brush.Color;

        var color = brush.Color;
        var center = (PreviewSize - 1) / 2f;

        // largest brush fills the preview, so relative sizes read correctly
        var previewRadius = brush.Radius / (float) BrushSettings.MaxRadius * (PreviewSize / 2f - 2f);

        var pixels = new Color[PreviewSize * PreviewSize];
        for (var i = 0; i < pixels.Length; i++)
        {
            var x = i % PreviewSize;
            var y = i / PreviewSize;

            var light = (x / CheckerSquare + y / CheckerSquare) % 2 == 0;
            var background = light ? new Color(0.24f, 0.26f, 0.27f) : new Color(0.17f, 0.19f, 0.20f);

            var dx = x - center;
            var dy = y - center;
            var distance = Mathf.Sqrt(dx * dx + dy * dy);

            if (previewRadius <= 0 || distance > previewRadius)
            {
                pixels[i] = background;
                continue;
            }

            // reuse the real falloff so the preview can't drift from the paint code
            var alpha = brush.AlphaAt(distance / previewRadius * brush.Radius);
            pixels[i] = Color.Lerp(background, color, Mathf.Clamp01(alpha));
        }

        Upload(brushPreview, pixels);
        return brushPreview;
    }

#if BRUSHES
    /// <summary>
    /// Swatch for a saved brush. Drawn white rather than in the current colour so it reads as a
    /// shape-and-softness sample, and so it never needs regenerating when the colour changes.
    /// </summary>
    public static Texture2D PresetPreview(BrushPreset preset)
    {
        if (PresetPreviews.TryGetValue(preset, out var cached) && cached) return cached;

        var stamp = new BrushStamp(Color.white, (byte) preset.Radius,
            (byte) Mathf.RoundToInt(preset.Opacity * 255f),
            (byte) Mathf.RoundToInt(preset.Hardness * 255f),
            preset.Shape);

        var texture = NewTexture(PreviewSize, PreviewSize);
        var pixels = new Color[PreviewSize * PreviewSize];

        var center = (PreviewSize - 1) / 2f;
        var extent = PreviewSize / 2f - 2f;

        for (var i = 0; i < pixels.Length; i++)
        {
            var x = i % PreviewSize;
            var y = i / PreviewSize;

            var light = (x / CheckerSquare + y / CheckerSquare) % 2 == 0;
            var background = light ? new Color(0.24f, 0.26f, 0.27f) : new Color(0.17f, 0.19f, 0.20f);

            // preset swatches always fill the tile, so shape and softness are comparable
            var nx = (x - center) / extent;
            var ny = (y - center) / extent;

            float alpha;
            if (preset.Shape == BrushShape.Custom)
            {
                alpha = (preset.Mask?.Sample(nx, ny) ?? 0f) * preset.Opacity;
            }
            else
            {
                var offset = preset.Shape == BrushShape.Square
                    ? Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny))
                    : Mathf.Sqrt(nx * nx + ny * ny);

                if (offset > 1f)
                {
                    pixels[i] = background;
                    continue;
                }

                alpha = stamp.FalloffFromNormalized(offset) * preset.Opacity;
            }
            pixels[i] = Color.Lerp(background, Color.white, Mathf.Clamp01(alpha));
        }

        Upload(texture, pixels);
        PresetPreviews[preset] = texture;

        return texture;
    }

#endif

    private static Texture2D CreateColorWheel()
    {
        var texture = NewTexture(WheelSize, WheelSize);

        var pixels = new Color[WheelSize * WheelSize];
        var center = (WheelSize - 1) / 2f;
        var outer = WheelSize / 2f - 1f;

        // baked rather than a CSS border: layout doesn't inset for borders, so the image would
        // simply paint over one drawn on the wrapper
        var ringInner = outer - WheelRingWidth;

        for (var i = 0; i < pixels.Length; i++)
        {
            // texture rows run bottom-up, which already matches the maths orientation
            var dx = i % WheelSize - center;
            var dy = i / WheelSize - center;

            var distance = Mathf.Sqrt(dx * dx + dy * dy);
            if (distance > outer)
            {
                pixels[i] = Color.clear;
                continue;
            }

            Color color;
            if (distance >= ringInner)
            {
                color = Color.white;
            }
            else
            {
                var hue = Mathf.Repeat(Mathf.Atan2(dy, dx) / (2f * Mathf.PI), 1f);
                color = Color.HSVToRGB(hue, Mathf.Clamp01(distance / ringInner), 1f);
            }

            // feather the last pixel so the rim isn't a hard staircase
            color.a = Mathf.Clamp01(outer - distance);
            pixels[i] = color;
        }

        Upload(texture, pixels);
        return texture;
    }

    private static Texture2D NewTexture(int width, int height) =>
        new(width, height, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

    private static void Upload(Texture2D texture, Color[] pixels)
    {
        texture.SetPixels(new Il2CppStructArray<Color>(pixels));
        texture.Apply();
    }
}
