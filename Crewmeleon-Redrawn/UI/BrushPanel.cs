using Crewmeleon_Redrawn.Buttons.Hider;
using Crewmeleon_Redrawn.Components;
using MiraAPI.Hud;
using Crewmeleon_Redrawn.Modifiers;
using MiraAPI.Modifiers;
using ReactUI.Core;
using ReactUI.Hooks;
using UnityEngine;
using S = ReactUI.Style;
using static ReactUI.UI;

namespace Crewmeleon_Redrawn.UI;

/// <summary>brush controls, only up while youre painting</summary>
public static class BrushPanel
{
    public const float WheelPx = 116f;
    public const float MarkerPx = 11f;

    // how much of ColorWheel.png carries colour, the rest is the white ring
    private const float WheelColorFraction = 120f / 127f;

    private const float AnchorLeft = 28f;
    private const float AnchorTop = 120f;

    private static readonly Func<VNode> Root = Component(RenderRoot);

    private static int componentId = -1;
    private static int lastVersion = -1;
    private static bool lastPainting;
    private static bool lastPicking;

    public static VNode Render() => Root();

    /// <summary>watches the state the panel draws from and redraws only when it changes</summary>
    public static void Tick()
    {
        if (componentId < 0) return;

        var painting = IsPainting();
        var picking = CustomButtonSingleton<PickColorButton>.Instance.IsPicking;
        var version = BrushStore.Local.Version;

        if (painting == lastPainting && picking == lastPicking && version == lastVersion) return;

        lastPainting = painting;
        lastPicking = picking;
        lastVersion = version;

        Scheduler.ScheduleRender(componentId);
    }

    private static VNode RenderRoot()
    {
        // the brush changes outside React, so a ticker watches it and only asks for a redraw when
        // something actually moved
        componentId = HooksRuntime.Current.ComponentId;

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

        // stop at the inside of the baked outline where the colour actually ends
        var reach = brush.Saturation * (WheelPx / 2f - 1f) * WheelColorFraction;

        // screen space is top down so Y gets subtracted here
        var markerX = WheelPx / 2f + Mathf.Cos(angle) * reach - MarkerPx / 2f;
        var markerY = WheelPx / 2f - Mathf.Sin(angle) * reach - MarkerPx / 2f;

        return PointerArea(p => ApplyWheelPointer(brush, p), ClassName("wheel-wrap"),
            Image(CrewmeleonAssets.ColorWheel.LoadAsset().texture, ClassName("wheel")),
            Div(ClassName("wheel-marker", new S.Style
            {
                Inset = new S.EdgeValues(markerY, float.NaN, float.NaN, markerX),
                Background = ToHex(brush.Color),
            }))
        );
    }

    /// <summary>turns a pointer position on the wheel into hue by angle and saturation by radius</summary>
    private static void ApplyWheelPointer(BrushSettings brush, Vector2 normalized)
    {
        var dx = normalized.x - 0.5f;

        // pointer space is top down, the wheel texture isnt
        var dy = 0.5f - normalized.y;

        brush.Hue = Mathf.Repeat(Mathf.Atan2(dy, dx) / (2f * Mathf.PI), 1f);
        brush.Saturation = Mathf.Sqrt(dx * dx + dy * dy) * 2f / WheelColorFraction;
    }

    /// <summary>value drags on the gradient itself so youre picking on a preview of the result</summary>
    private static VNode ValueRow(BrushSettings brush)
    {
        var lit = ToHex(Color.HSVToRGB(brush.Hue, brush.Saturation, 1f));

        return Div(ClassName("gap-6"),
            Div(ClassName("row-between"),
                Text("Value", ClassName("text-label")),
                Text(Percent(brush.Value), ClassName("text-value"))
            ),
            PointerArea(p => brush.Value = Mathf.Clamp01(p.x), ClassName("strip-wrap", new S.Style
            {
                BackgroundGradient = new S.Gradient
                {
                    Type = S.GradientType.Linear,
                    Angle = 0f,
                    ColorA = "#000000",
                    ColorB = lit,
                },
            }),
                // percent spacer instead of an absolute offset, since the strip stretches and
                // Inset only takes pixels
                Div(ClassName("strip-row"),
                    Div(ClassName("strip-spacer", new S.Style { Width = S.StyleValue.Percent(brush.Value * 100f) })),
                    Div(ClassName("strip-marker"))
                )
            )
        );
    }

    private static VNode BrushSection(BrushSettings brush)
    {
        return Div(ClassName("section"),
            Text("BRUSH", ClassName("section-title")),
            Div(ClassName("row gap-10"),
                Div(ClassName("preview-frame"), Image(BrushTextures.BrushPreview(brush), ClassName("preview"))),
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
            Keybind("Ctrl + Scroll", "Brush size"),
            Keybind("Ctrl + Z", "Undo last stroke"),
            Keybind("Hold Space", "Pick colour on release")
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
