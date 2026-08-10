using Crewmeleon_Redrawn.Components;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using ReactUI.Core;
using UnityEngine;
using S = ReactUI.Style;
using static ReactUI.UI;

namespace Crewmeleon_Redrawn.UI;

/// <summary>
/// Draw-your-own brush tip. The canvas is a transparent grid so the tip's own alpha is what the
/// player is shaping.
/// </summary>
public static class BrushCreator
{
    public const float CanvasPx = 210f;

    private const int CheckerSquare = 7;

    public static bool IsOpen { get; private set; }

    private static BrushMask mask = new();
    private static bool erasing;
    private static Texture2D? canvasTexture;
    private static bool textureDirty = true;

    public static void Open()
    {
        mask = new BrushMask();
        erasing = false;
        textureDirty = true;
        IsOpen = true;
    }

    public static void Close() => IsOpen = false;

    public static VNode Render()
    {
        return Div(ClassName("section"),
            Div(ClassName("row-between"),
                Text("NEW BRUSH", ClassName("section-title")),
                Text(erasing ? "ERASING" : "DRAWING", ClassName("creator-mode"))
            ),
            PointerArea(Paint, ClassName("creator-canvas"),
                Image(CanvasTexture(), ClassName("creator-img"))
            ),
            Div(ClassName("row gap-6"),
                Button(erasing ? "Draw" : "Erase", () => erasing = !erasing, ClassName("btn btn-small btn-busy")),
                Button("Clear", Clear, ClassName("btn btn-small btn-busy")),
                Button("Cancel", Close, ClassName("btn btn-small btn-busy")),
                Button("Save", Save, ClassName("btn btn-small btn-accent"))
            )
        );
    }

    private static void Paint(Vector2 normalized)
    {
        if (normalized.x is < 0f or > 1f || normalized.y is < 0f or > 1f) return;

        // pointer space is top-down, the mask is not
        var x = Mathf.Min((int) (normalized.x * BrushMask.Size), BrushMask.Size - 1);
        var y = Mathf.Min((int) ((1f - normalized.y) * BrushMask.Size), BrushMask.Size - 1);

        var value = (byte) (erasing ? 0 : 255);
        var index = y * BrushMask.Size + x;

        if (mask.Cells[index] == value) return;

        mask.Cells[index] = value;
        textureDirty = true;
    }

    private static void Clear()
    {
        Array.Clear(mask.Cells, 0, mask.Cells.Length);
        textureDirty = true;
    }

    private static void Save()
    {
        if (mask.IsEmpty) return;

        var preset = BrushLibrary.AddCustom(mask.Clone());
        preset.ApplyTo(BrushStore.Local);

        Close();
    }

    private static Texture2D CanvasTexture()
    {
        canvasTexture ??= new Texture2D(BrushMask.Size, BrushMask.Size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        if (!textureDirty) return canvasTexture;
        textureDirty = false;

        var pixels = new Color[BrushMask.Size * BrushMask.Size];
        for (var i = 0; i < pixels.Length; i++)
        {
            var x = i % BrushMask.Size;

            // both the texture and the mask run bottom-up, so these indices line up directly
            var y = i / BrushMask.Size;

            var light = (x / CheckerSquare + y / CheckerSquare) % 2 == 0;
            var background = light ? new Color(0.24f, 0.26f, 0.27f) : new Color(0.17f, 0.19f, 0.20f);

            var alpha = mask.Cells[y * BrushMask.Size + x] / 255f;
            pixels[i] = Color.Lerp(background, Color.white, alpha);
        }

        canvasTexture.SetPixels(new Il2CppStructArray<Color>(pixels));
        canvasTexture.Apply();

        return canvasTexture;
    }
}
