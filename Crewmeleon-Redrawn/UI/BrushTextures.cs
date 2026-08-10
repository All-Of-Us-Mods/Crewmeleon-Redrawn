using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace Crewmeleon_Redrawn.UI;

public static class BrushTextures
{
    private const int HueStripWidth = 256;

    private static Texture2D? hueStrip;

    public static Texture2D HueStrip => hueStrip ??= CreateHueStrip();

    private static Texture2D CreateHueStrip()
    {
        var texture = new Texture2D(HueStripWidth, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        var pixels = new Color[HueStripWidth];
        for (var x = 0; x < HueStripWidth; x++)
        {
            pixels[x] = Color.HSVToRGB(x / (float) (HueStripWidth - 1), 1f, 1f);
        }

        texture.SetPixels(new Il2CppStructArray<Color>(pixels));
        texture.Apply();

        return texture;
    }
}
