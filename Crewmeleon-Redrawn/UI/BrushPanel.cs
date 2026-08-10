using Crewmeleon_Redrawn.Components;
using Crewmeleon_Redrawn.Modifiers;
using MiraAPI.Modifiers;
using ReactUI.Core;
using ReactUI.Hooks;
using UnityEngine;
using S = ReactUI.Style;
using static ReactUI.UI;

namespace Crewmeleon_Redrawn.UI;

/// <summary>
/// Brush controls, shown while the local player is painting.
/// </summary>
public static class BrushPanel
{
    public const float WheelPx = 116f;
    public const float MarkerPx = 11f;

    private const float AnchorLeft = 28f;
    private const float AnchorTop = 120f;

    private static readonly Func<VNode> Root = Component(RenderRoot);

    public static VNode Render() => Root();

    private static VNode RenderRoot()
    {
        // the brush is mutated outside React by drags and the eyedropper, so redraw every frame
        Scheduler.ScheduleRender(HooksRuntime.Current.ComponentId);

        if (!IsPainting()) return Div();

        var brush = BrushStore.Local;

        return Div(ClassName("panel", new S.Style
        {
            Inset = new S.EdgeValues(AnchorTop, float.NaN, float.NaN, AnchorLeft),
        }),
            Div(ClassName("panel-body"),
                ColorSection(brush),
                BrushSection(brush),
                KeybindsSection()
            )
        );
    }

    private static VNode ColorSection(BrushSettings brush)
    {
        return Div(ClassName("section"),
            Text("COLOUR", ClassName("section-title")),
            Div(ClassName("row gap-10"),
                Wheel(brush),
                Div(ClassName("swatch-col"),
                    Div(ClassName("swatch-well"),
                        Div(ClassName("swatch", new S.Style
                        {
                            Background = ToHex(brush.Color),
                            Opacity = Mathf.Max(brush.Opacity, 0.08f),
                        }))
                    ),
                    Text(ToHex(brush.Color), ClassName("hex-text"))
                )
            ),
            ValueRow(brush)
        );
    }

    private static VNode Wheel(BrushSettings brush)
    {
        var angle = brush.Hue * 2f * Mathf.PI;
        var reach = brush.Saturation * (WheelPx / 2f - 1f);

        // screen space is top-down, so the marker's Y is subtracted rather than added
        var markerX = WheelPx / 2f + Mathf.Cos(angle) * reach - MarkerPx / 2f;
        var markerY = WheelPx / 2f - Mathf.Sin(angle) * reach - MarkerPx / 2f;

        return PointerArea(p => ApplyWheelPointer(brush, p), ClassName("wheel-wrap"),
            Image(BrushTextures.ColorWheel, ClassName("wheel")),
            Div(ClassName("wheel-marker", new S.Style
            {
                Inset = new S.EdgeValues(markerY, float.NaN, float.NaN, markerX),
                Background = ToHex(brush.Color),
            }))
        );
    }

    /// <summary>Maps a normalized pointer position on the wheel to hue (angle) and saturation (radius).</summary>
    private static void ApplyWheelPointer(BrushSettings brush, Vector2 normalized)
    {
        var dx = normalized.x - 0.5f;

        // pointer space is top-down, the wheel texture is not
        var dy = 0.5f - normalized.y;

        brush.Hue = Mathf.Repeat(Mathf.Atan2(dy, dx) / (2f * Mathf.PI), 1f);
        brush.Saturation = Mathf.Sqrt(dx * dx + dy * dy) * 2f;
    }

    /// <summary>Value runs on the gradient itself rather than a slider, so it previews the result.</summary>
    private static VNode ValueRow(BrushSettings brush)
    {
        return Div(ClassName("gap-6"),
            Div(ClassName("row-between"),
                Text("Value", ClassName("text-label")),
                Text(Percent(brush.Value), ClassName("text-value"))
            ),
            PointerArea(p => brush.Value = Mathf.Clamp01(p.x), ClassName("strip-wrap"),
                Image(BrushTextures.ValueStrip(brush), ClassName("strip"))
            )
        );
    }

    private static VNode BrushSection(BrushSettings brush)
    {
        return Div(ClassName("section"),
            Text("BRUSH", ClassName("section-title")),
            Div(ClassName("row gap-10"),
                Image(BrushTextures.BrushPreview(brush), ClassName("preview")),
                Div(ClassName("grow gap-6"),
                    SliderRow("Size", brush.Radius, v => brush.Radius = Mathf.RoundToInt(v), $"{brush.Radius}px",
                        BrushSettings.MinRadius, BrushSettings.MaxRadius, step: 1f),
                    SliderRow("Opacity", brush.Opacity, v => brush.Opacity = v, Percent(brush.Opacity)),
                    SliderRow("Hardness", brush.Hardness, v => brush.Hardness = v, Percent(brush.Hardness))
                )
            )
        );
    }

    private static VNode SliderRow(
        string label,
        float value,
        Action<float> onChange,
        string display,
        float min = 0f,
        float max = 1f,
        float step = 0f)
    {
        return Div(ClassName("setting-row-slider"),
            Text(label, ClassName("text-label")),
            Div(ClassName("grow"),
                Slider(value, onChange, min, max, ClassName("slider-control"), step,
                    thumbWidth: 7f, thumbHeight: 18f, thumbRadius: 2f)
            ),
            Text(display, ClassName("text-value"))
        );
    }

    private static VNode KeybindsSection()
    {
        return Div(ClassName("section"),
            Text("KEYBINDS", ClassName("section-title")),
            Keybind("Left Click", "Paint"),
            Keybind("Scroll", "Zoom in / out"),
            Keybind("Ctrl + Scroll", "Brush size")
        );
    }

    private static VNode Keybind(string key, string action)
    {
        return Div(ClassName("keybind-row"),
            Div(ClassName("keybind-key"), Text(key, ClassName("keybind-key-text"))),
            Text(action, ClassName("keybind-action"))
        );
    }

    private static string Percent(float value) => $"{Mathf.RoundToInt(value * 100)}%";

    private static bool IsPainting()
    {
        var player = PlayerControl.LocalPlayer;
        return player && player.Data?.Role != null && player.HasModifier<PaintingModifier>();
    }

    private static string ToHex(Color color)
    {
        return $"#{Mathf.RoundToInt(color.r * 255):X2}{Mathf.RoundToInt(color.g * 255):X2}{Mathf.RoundToInt(color.b * 255):X2}";
    }
}
