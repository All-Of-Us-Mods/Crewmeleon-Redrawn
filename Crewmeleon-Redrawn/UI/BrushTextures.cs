using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace Crewmeleon_Redrawn.UI;

public static class BrushTextures
{
    public const int WheelSize = 256;

    private static Texture2D? colorWheel;

    public static Texture2D ColorWheel => colorWheel ??= CreateColorWheel();

    /// <summary>
    /// Hue around the circumference, saturation along the radius. Value is left at 1 and
    /// controlled separately, so the wheel stays readable at any brightness.
    /// </summary>
    private static Texture2D CreateColorWheel()
    {
        var texture = new Texture2D(WheelSize, WheelSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        var pixels = new Color[WheelSize * WheelSize];
        var center = (WheelSize - 1) / 2f;
        var outer = WheelSize / 2f - 1f;

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

            var hue = Mathf.Repeat(Mathf.Atan2(dy, dx) / (2f * Mathf.PI), 1f);
            var color = Color.HSVToRGB(hue, Mathf.Clamp01(distance / outer), 1f);

            // feather the last pixel so the rim isn't a hard staircase
            color.a = Mathf.Clamp01(outer - distance);
            pixels[i] = color;
        }

        texture.SetPixels(new Il2CppStructArray<Color>(pixels));
        texture.Apply();

        return texture;
    }
}
