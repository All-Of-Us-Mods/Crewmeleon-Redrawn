using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace Crewmeleon_Redrawn.Utilities;

/// <summary>
/// Generates circular cursor sprites. The rim is baked black so a tinted sprite
/// stays readable against any body colour.
/// </summary>
public static class CircleSprite
{
    private const int TextureSize = 64;
    private const float RimThickness = 1f;

    /// <summary>
    /// The drawn circle stops one pixel short of the sprite bounds, so callers that need the
    /// visible edge to land on an exact world size must divide their scale by this.
    /// </summary>
    public const float DrawnDiameterFraction = (TextureSize - 2f) / TextureSize;

    public static Sprite CreateRing(float thickness)
    {
        var outer = TextureSize / 2f - 1f;
        return Create(outer - thickness, outer);
    }

    public static Sprite CreateDisc()
    {
        return Create(0f, TextureSize / 2f - 1f);
    }

    private static Sprite Create(float innerRadius, float outerRadius)
    {
        var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        var pixels = new Color[TextureSize * TextureSize];
        var center = (TextureSize - 1) / 2f;

        for (var i = 0; i < pixels.Length; i++)
        {
            var dx = i % TextureSize - center;
            var dy = i / TextureSize - center;
            var distance = Mathf.Sqrt(dx * dx + dy * dy);

            if (distance > outerRadius || distance < innerRadius)
                pixels[i] = Color.clear;
            else if (distance > outerRadius - RimThickness || (innerRadius > 0 && distance < innerRadius + RimThickness))
                pixels[i] = Color.black;
            else
                pixels[i] = Color.white;
        }

        texture.SetPixels(new Il2CppStructArray<Color>(pixels));
        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, TextureSize, TextureSize),
            new Vector2(0.5f, 0.5f),
            TextureSize);
    }
}
