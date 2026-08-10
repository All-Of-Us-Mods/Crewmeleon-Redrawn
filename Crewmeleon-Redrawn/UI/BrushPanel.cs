using Crewmeleon_Redrawn.Buttons.Hider;
using Crewmeleon_Redrawn.Components;
using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Utilities;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using ReactUI.Core;
using UnityEngine;
using S = ReactUI.Style;
using static ReactUI.UI;

namespace Crewmeleon_Redrawn.UI;

/// <summary>
/// Brush controls shown while the local player is painting.
/// </summary>
public static class BrushPanel
{
    private const string Background = "#1a1a2eE8";
    private const string Panel = "#2d2a33";
    private const string Muted = "#a0a0a0";
    private const string Accent = "#7c3aed";

    private static readonly Func<VNode> Root = Component(RenderRoot);

    public static VNode Render() => Root();

    private static VNode RenderRoot()
    {
        var (posX, setPosX) = UseState(24f);
        var (posY, setPosY) = UseState(120f);

        // re-render every frame so slider drags and eyedropper picks show immediately
        Scheduler.ScheduleRender(ReactUI.Hooks.HooksRuntime.Current.ComponentId);

        if (!IsPainting()) return Div();

        var brush = BrushStore.Local;

        var capturedX = posX;
        var capturedY = posY;
        ReactUI.Input.InputSystem.RegisterDraggable(
            ReactUI.Hooks.HooksRuntime.Current.ComponentId,
            () => capturedX, () => capturedY, setPosX, setPosY);

        return Div(new S.Style
        {
            Position = S.PositionType.Absolute,
            Inset = new S.EdgeValues(posY, float.NaN, float.NaN, posX),
            Width = 260,
            Background = Background,
            BorderRadius = 14,
            BorderColor = "#ffffff15",
            BorderWidth = 1,
            BoxShadow = new S.BoxShadow { Blur = 12, Color = "rgba(0,0,0,0.5)" },
            Padding = new S.EdgeValues(16),
            Gap = 12,
            Cursor = S.CursorType.Pointer,
        },
            Header(brush),
            HueRow(brush),
            SliderRow("Saturation", brush.Saturation, v => brush.Saturation = v),
            SliderRow("Value", brush.Value, v => brush.Value = v),
            SliderRow("Opacity", brush.Opacity, v => brush.Opacity = v),
            SliderRow("Hardness", brush.Hardness, v => brush.Hardness = v),
            SliderRow($"Size  {brush.Radius}px", (brush.Radius - BrushSettings.MinRadius) / (float) (BrushSettings.MaxRadius - BrushSettings.MinRadius),
                v => brush.Radius = Mathf.RoundToInt(Mathf.Lerp(BrushSettings.MinRadius, BrushSettings.MaxRadius, v))),
            EyedropperButton()
        );
    }

    private static VNode Header(BrushSettings brush)
    {
        return Div(new S.Style
        {
            FlexDirection = S.FlexDirection.Row,
            AlignItems = S.AlignItems.Center,
            Gap = 10,
        },
            Div(new S.Style
            {
                Width = S.StyleValue.Px(34),
                Height = S.StyleValue.Px(34),
                Background = ToHex(brush.Color),
                BorderRadius = 8,
                BorderWidth = 1,
                BorderColor = "#ffffff30",
                Opacity = brush.Opacity,
            }),
            Text("Brush", new S.Style { FontSize = 17, FontWeight = 700, Color = "#e0e0e0", FlexGrow = 1 })
        );
    }

    private static VNode HueRow(BrushSettings brush)
    {
        return Div(new S.Style { Gap = 4 },
            Label("Hue"),
            Image(BrushTextures.HueStrip, new S.Style
            {
                Height = S.StyleValue.Px(10),
                BorderRadius = 5,
            }),
            Slider(brush.Hue, v => brush.Hue = v, 0f, 1f, SliderStyle())
        );
    }

    private static VNode SliderRow(string label, float value, Action<float> onChange)
    {
        return Div(new S.Style { Gap = 4 },
            Label(label),
            Slider(value, onChange, 0f, 1f, SliderStyle())
        );
    }

    private static VNode Label(string text) =>
        Text(text, new S.Style { FontSize = 12, Color = Muted });

    private static S.Style SliderStyle() => new()
    {
        Height = S.StyleValue.Px(18),
        Background = Panel,
        BorderRadius = 9,
        Cursor = S.CursorType.Pointer,
    };

    private static VNode EyedropperButton()
    {
        var picker = CustomButtonSingleton<PickColorButton>.Instance;
        var picking = picker.IsPicking;

        return Button(picking ? "Picking…" : "Pick from screen", picker.BeginPick, new S.Style
        {
            Padding = new S.EdgeValues(8, 14),
            Background = picking ? "#3d3a43" : Accent,
            Color = "#ffffff",
            BorderRadius = 8,
            FontSize = 13,
            FontWeight = 600,
            AlignItems = S.AlignItems.Center,
            JustifyContent = S.JustifyContent.Center,
            Cursor = S.CursorType.Pointer,
            Hover = new S.Style { Background = picking ? "#3d3a43" : "#6d28d9" },
            Transitions = new[] { new S.Transition { Property = "background", Duration = 0.15f, Easing = S.EasingType.Ease } },
        });
    }

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
