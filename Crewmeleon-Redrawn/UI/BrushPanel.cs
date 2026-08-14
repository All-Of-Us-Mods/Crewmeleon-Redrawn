using MiraAPI.GameOptions;
using MiraAPI.Hud;
using CrewmeleonRedrawn.Utilities;
using CrewmeleonRedrawn.Buttons.Hider;
using CrewmeleonRedrawn.Components;
using CrewmeleonRedrawn.Modifiers;
using MiraAPI.Modifiers;
using ReactUI.Core;
using ReactUI.Hooks;
using UnityEngine;
using S = ReactUI.Style;
using static ReactUI.UI;

namespace CrewmeleonRedrawn.UI;

/// <summary>brush controls, only up while youre painting</summary>
public static class BrushPanel
{
    /// <summary>mobile stacks the wheel on its own line, so it takes the section's full width</summary>
    public static float WheelPx =>
        MobileUi.Active ? CrewmeleonStyles.SectionContentWidth : CrewmeleonStyles.Px(116f);
    public static float MarkerPx => CrewmeleonStyles.Px(11f);

    // how much of ColorWheel.png carries colour, the rest is the white ring
    private const float WheelColorFraction = 120f / 127f;

    private static readonly Func<VNode> ColourRoot = Component(RenderColour);
    private static readonly Func<VNode> BrushRoot = Component(RenderBrush);
    private static readonly Func<VNode> FullRoot = Component(RenderFull);

    private static readonly List<int> componentIds = [];

    private static int lastVersion = -1;
    private static bool lastPainting;
    private static bool lastPicking;

    /// <summary>
    /// Mobile splits into two roots so neither covers the screen and blocks game clicks. All three
    /// mount up front and pick themselves out by layout, so the dev toggle can switch live.
    /// </summary>
    public static void Mount()
    {
        Render(ColourRoot);
        Render(BrushRoot);
        Render(FullRoot);
    }

    /// <summary>watches the state the panel draws from and redraws only when it changes</summary>
    public static void Tick()
    {
        if (componentIds.Count == 0) return;

        var toggled = MobileUi.PollToggle();
        if (toggled) CrewmeleonStyles.Register();

        var resized = CrewmeleonStyles.RefreshIfResolutionChanged();

        var painting = IsPainting();
        var picking = CustomButtonSingleton<PickColorButton>.Instance.IsPicking;
        var version = BrushStore.Local.Version;

        if (!toggled && !resized
            && painting == lastPainting && picking == lastPicking && version == lastVersion) return;

        lastPainting = painting;
        lastPicking = picking;
        lastVersion = version;

        foreach (var id in componentIds) Scheduler.ScheduleRender(id);
    }

    private static VNode RenderColour() =>
        Visible(MobileUi.Active) ? Panel(LeftInset, ColorSection(BrushStore.Local)) : Div();

    private static VNode RenderBrush() =>
        Visible(MobileUi.Active) ? Panel(RightInset, BrushSection(BrushStore.Local)) : Div();

    private static VNode RenderFull()
    {
        if (!Visible(!MobileUi.Active)) return Div();

        var brush = BrushStore.Local;
        return Panel(LeftInset, ColorSection(brush), BrushSection(brush), KeybindsSection());
    }

    /// <summary>the brush changes outside React, so each root registers for the ticker's redraws</summary>
    private static bool Visible(bool inThisLayout)
    {
        var id = HooksRuntime.Current.ComponentId;
        if (!componentIds.Contains(id)) componentIds.Add(id);

        return inThisLayout && IsPainting();
    }

    private static S.EdgeValues LeftInset =>
        new(CrewmeleonStyles.ScreenGutter, float.NaN, float.NaN, CrewmeleonStyles.ScreenGutter);

    private static S.EdgeValues RightInset =>
        new(CrewmeleonStyles.ScreenGutter, CrewmeleonStyles.ScreenGutter, float.NaN, float.NaN);

    private static VNode Panel(S.EdgeValues inset, params VNode[] sections)
    {
        return Div(ClassName("panel", new S.Style { Inset = inset }),
            Div(ClassName("panel-body"), sections)
        );
    }

    private static VNode ColorSection(BrushSettings brush)
    {
        if (MobileUi.Active)
        {
            return Div(ClassName("section"),
                Text("COLOUR", ClassName("section-title")),
                Div(ClassName("gap-10"),
                    Div(ClassName("self-center"), Wheel(brush)),
                    ValueRow(brush)
                )
            );
        }

        return Div(ClassName("section"),
            Text("COLOUR", ClassName("section-title")),
            Div(ClassName("row gap-10 items-top"),
                Wheel(brush),
                Div(ClassName("swatch-col"), Preview(brush), ValueRow(brush))
            )
        );
    }

    private static VNode Preview(BrushSettings brush) =>
        Div(ClassName("swatch-well"), Image(BrushTextures.BrushPreview(brush), ClassName("swatch")));

    private static VNode Wheel(BrushSettings brush)
    {
        var angle = brush.Hue * 2f * Mathf.PI;

        // stop at the inside of the baked outline where the colour actually ends
        var reach = brush.Saturation * (WheelPx / 2f - CrewmeleonStyles.Px(1f)) * WheelColorFraction;

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
        // stacked rows need more air between them than inside them, or the labels read as
        // belonging to the slider above
        var sliders = Div(ClassName(MobileUi.Active ? "gap-10" : "gap-6"),
            SliderRow("Size", brush.Radius, v => brush.Radius = Mathf.RoundToInt(v), $"{brush.Radius}px",
                BrushSettings.MinRadius, BrushSettings.MaxRadius, step: 1f),
            SliderRow("Opacity", brush.Opacity, v => brush.Opacity = v, Percent(brush.Opacity)),
            SliderRow("Hardness", brush.Hardness, v => brush.Hardness = v, Percent(brush.Hardness))
        );

        // desktop undoes with Ctrl + Z, so the button is only earning its space on touch
        if (MobileUi.Active)
        {
            var children = new List<VNode>
            {
                Text("BRUSH", ClassName("section-title")),
                Preview(brush),
                sliders,
            };

            if (OptionGroupSingleton<GameplayOptions>.Instance.AllowUndo.Value)
                children.Add(Button("UNDO", UndoLastStroke, ClassName("btn btn-dark")));

            return Div(ClassName("section"), children);
        }

        return Div(ClassName("section"),
            Text("BRUSH", ClassName("section-title")),
            sliders
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
        var slider = Slider(value, onChange, min, max, ClassName("slider-control"), step,
            thumbWidth: CrewmeleonStyles.SliderThumbWidthPx,
            thumbHeight: CrewmeleonStyles.SliderThumbHeightPx,
            thumbRadius: CrewmeleonStyles.SliderThumbRadiusPx,
            trackHeight: CrewmeleonStyles.SliderTrackHeightPx);

        if (MobileUi.Active)
        {
            return Div(ClassName("gap-6"),
                Div(ClassName("row-between"),
                    Text(label, ClassName("text-label")),
                    Text(display, ClassName("text-value"))
                ),
                slider
            );
        }

        return Div(ClassName("setting-row-slider"),
            Text(label, ClassName("text-label slider-label")),
            Div(ClassName("grow"), slider),
            Text(display, ClassName("text-value"))
        );
    }

    private static VNode KeybindsSection()
    {
        var rows = new List<VNode>
        {
            Text("KEYBINDS", ClassName("section-title")),
            Keybind("Left Click", "Paint"),
            Keybind("Scroll", "Zoom in / out"),
            Keybind("Ctrl + Scroll", "Brush size"),
            Keybind("Hold Space", "Pick colour"),
        };

        if (OptionGroupSingleton<GameplayOptions>.Instance.AllowUndo.Value)
            rows.Add(Keybind("Ctrl + Z", "Undo last stroke"));

        return Div(ClassName("section"), rows);
    }

    private static VNode Keybind(string key, string action)
    {
        return Div(ClassName("keybind-row"),
            Div(ClassName("keybind-key"), Text(key, ClassName("keybind-key-text"))),
            Text(action, ClassName("keybind-action"))
        );
    }

    private static void UndoLastStroke()
    {
        var player = PlayerControl.LocalPlayer;
        if (player && player.GetPlayerCanvas(out var canvas)) canvas!.UndoLastLocalStroke();
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
