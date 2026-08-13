using CrewmeleonRedrawn.Components;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace CrewmeleonRedrawn.UI;

/// <summary>
/// the brush preview stays generated because it has to show the real falloff. everything else
/// static lives in Resources as a sprite
/// </summary>
public static class BrushTextures
{
    private const int PreviewSize = 72;
    private const int CheckerSquare = 6;

    private static Texture2D? brushPreview;
    private static Vector4 lastState = new(-1, -1, -1, -1);
    private static Color lastColor = new(-1, -1, -1);

    /// <summary>checkerboard with the brush drawn centred on it, using the same falloff as painting</summary>
    public static Texture2D BrushPreview(BrushSettings brush)
    {
        brushPreview ??= new Texture2D(PreviewSize, PreviewSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        var state = new Vector4(brush.Radius, brush.Hardness, brush.Opacity, 0);
        if (state == lastState && brush.Color == lastColor) return brushPreview;
        lastState = state;
        lastColor = brush.Color;

        var color = brush.Color;
        var center = (PreviewSize - 1) / 2f;

        // biggest brush fills the preview so relative sizes read correctly
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

            // reuse the real falloff so the preview cant drift from the paint code
            var alpha = brush.AlphaAt(distance / previewRadius * brush.Radius);
            pixels[i] = Color.Lerp(background, color, Mathf.Clamp01(alpha));
        }

        brushPreview.SetPixels(new Il2CppStructArray<Color>(pixels));
        brushPreview.Apply();

        return brushPreview;
    }
}
