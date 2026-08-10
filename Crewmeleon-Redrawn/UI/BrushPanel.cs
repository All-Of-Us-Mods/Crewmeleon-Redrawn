using Crewmeleon_Redrawn.Buttons.Hider;
using Crewmeleon_Redrawn.Components;
using Crewmeleon_Redrawn.Modifiers;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using ReactUI.Core;
using ReactUI.Hooks;
using ReactUI.Input;
using UnityEngine;
using S = ReactUI.Style;
using static ReactUI.UI;
using C = Crewmeleon_Redrawn.UI.CrewmeleonStyles;

namespace Crewmeleon_Redrawn.UI;

/// <summary>
/// Brush controls, shown while the local player is painting.
/// </summary>
public static class BrushPanel
{
    private static readonly Func<VNode> Root = Component(RenderRoot);

    public static VNode Render() => Root();

    private static VNode RenderRoot()
    {
        var (posX, setPosX) = UseState(28f);
        var (posY, setPosY) = UseState(140f);

        // slider drags and eyedropper picks mutate the brush outside React, so redraw every frame
        Scheduler.ScheduleRender(HooksRuntime.Current.ComponentId);

        if (!IsPainting()) return Div();

        InputSystem.RegisterDraggable(
            HooksRuntime.Current.ComponentId,
            () => posX, () => posY, setPosX, setPosY);

        var brush = BrushStore.Local;

        var inner = Div(ClassName("panel-inner"),
            TitleBar(),
            Div(ClassName("panel-body"),
                ColorSection(brush),
                BrushSection(brush)
            )
        );

        return Div(ClassName("panel-outer", new S.Style
        {
            Inset = new S.EdgeValues(posY, float.NaN, float.NaN, posX),
        }),
            inner,
            Div(ClassName("footer-anchor"), Footer())
        );
    }

    private static VNode TitleBar()
    {
        return Div(ClassName("title-bar"),
            Text("Brush", ClassName("title-text")),
            Text("drag to move", ClassName("title-hint"))
        );
    }

    private static VNode ColorSection(BrushSettings brush)
    {
        return Div(ClassName("section"),
            Text("COLOUR", ClassName("section-title")),
            Div(ClassName("row gap-10"),
                Div(ClassName("swatch-well"),
                    Div(ClassName("swatch", new S.Style
                    {
                        Background = ToHex(brush.Color),
                        Opacity = Mathf.Max(brush.Opacity, 0.08f),
                    }))
                ),
                Div(ClassName("grow gap-6"),
                    Image(BrushTextures.HueStrip, ClassName("hue-strip")),
                    Slider(brush.Hue, v => brush.Hue = v, 0f, 1f, ClassName("slider-control"))
                )
            ),
            SliderRow("Saturation", brush.Saturation, v => brush.Saturation = v, Percent(brush.Saturation)),
            SliderRow("Value", brush.Value, v => brush.Value = v, Percent(brush.Value)),
            Div(ClassName("divider")),
            EyedropperButton()
        );
    }

    private static VNode BrushSection(BrushSettings brush)
    {
        var sizeNormalized = (brush.Radius - BrushSettings.MinRadius)
                             / (float) (BrushSettings.MaxRadius - BrushSettings.MinRadius);

        return Div(ClassName("section"),
            Text("BRUSH", ClassName("section-title")),
            SliderRow("Size", sizeNormalized,
                v => brush.Radius = Mathf.RoundToInt(Mathf.Lerp(BrushSettings.MinRadius, BrushSettings.MaxRadius, v)),
                $"{brush.Radius}px"),
            SliderRow("Opacity", brush.Opacity, v => brush.Opacity = v, Percent(brush.Opacity)),
            SliderRow("Hardness", brush.Hardness, v => brush.Hardness = v, Percent(brush.Hardness))
        );
    }

    private static VNode SliderRow(string label, float value, Action<float> onChange, string display)
    {
        return Div(ClassName("setting-row-slider"),
            Text(label, ClassName("text-label")),
            Div(ClassName("grow"),
                Slider(value, onChange, 0f, 1f, ClassName("slider-control"))
            ),
            Text(display, ClassName("text-value"))
        );
    }

    private static VNode EyedropperButton()
    {
        var picker = CustomButtonSingleton<PickColorButton>.Instance;
        var picking = picker.IsPicking;

        return Button(
            picking ? "Click to sample…" : "Pick from screen",
            picker.BeginPick,
            ClassName(picking ? "btn btn-busy" : "btn btn-accent"));
    }

    private static VNode Footer()
    {
        return Div(ClassName("footer"),
            Text("Ctrl + scroll resizes the brush", ClassName("footer-text"))
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
