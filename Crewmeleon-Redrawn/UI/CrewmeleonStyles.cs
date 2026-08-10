using S = ReactUI.Style;

namespace Crewmeleon_Redrawn.UI;

/// <summary>
/// Among Us styling — near-black fills inside heavy white outlines, with the game mode's green
/// as the only accent.
/// </summary>
public static class CrewmeleonStyles
{
    public static readonly S.UIColor BgMain = "#08080a";
    public static readonly S.UIColor BgPanel = "#17171c";
    public static readonly S.UIColor BgSunken = "#000000";

    public static readonly S.UIColor Outline = "#ffffff";
    public static readonly S.UIColor OutlineSoft = "rgba(255,255,255,0.35)";

    public static readonly S.UIColor Accent = "#96ff5a";
    public static readonly S.UIColor OnAccent = "#0a1204";

    public static readonly S.UIColor TextPrimary = "#ffffff";
    public static readonly S.UIColor TextMuted = "#9a9aa6";

    public const float PanelWidth = 272f;
    public const float OutlineWidth = 3f;

    public static void Register()
    {
        var s = new S.StyleSheet();

        // ── Layout ───────────────────────────────────────────────
        s[".row"] = new S.Style { FlexDirection = S.FlexDirection.Row, AlignItems = S.AlignItems.Center };
        s[".row-between"] = new S.Style
        {
            FlexDirection = S.FlexDirection.Row,
            JustifyContent = S.JustifyContent.SpaceBetween,
            AlignItems = S.AlignItems.Center,
        };
        s[".grow"] = new S.Style { FlexGrow = 1 };
        s[".gap-6"] = new S.Style { Gap = 6 };
        s[".gap-10"] = new S.Style { Gap = 10 };

        // ── Panel ────────────────────────────────────────────────
        s[".panel"] = new S.Style
        {
            Position = S.PositionType.Absolute,
            Width = PanelWidth,
            Background = BgMain,
            BorderRadius = 16,
            BorderWidth = OutlineWidth,
            BorderColor = Outline,
            Overflow = S.Overflow.Hidden,
        };
        s[".panel-body"] = new S.Style { Padding = new S.EdgeValues(12), Gap = 10 };

        // ── Section ──────────────────────────────────────────────
        s[".section"] = new S.Style
        {
            Background = BgPanel,
            BorderRadius = 10,
            BorderWidth = 2,
            BorderColor = OutlineSoft,
            Padding = new S.EdgeValues(10, 12),
            Gap = 8,
        };
        s[".section-title"] = new S.Style { FontSize = 11, FontWeight = 700, Color = TextMuted };

        // ── Text ─────────────────────────────────────────────────
        s[".text-label"] = new S.Style { FontSize = 12, FontWeight = 700, Color = TextPrimary, FlexShrink = 0 };
        s[".text-value"] = new S.Style
        {
            FontSize = 11,
            FontWeight = 700,
            Color = TextMuted,
            Width = S.StyleValue.Px(34),
            FlexShrink = 0,
            TextAlign = S.TextAlign.Right,
        };
        s[".hex-text"] = new S.Style
        {
            FontSize = 11,
            FontWeight = 700,
            Color = TextMuted,
            TextAlign = S.TextAlign.Center,
        };

        // ── Sliders ──────────────────────────────────────────────
        s[".setting-row-slider"] = new S.Style
        {
            FlexDirection = S.FlexDirection.Row,
            JustifyContent = S.JustifyContent.SpaceBetween,
            AlignItems = S.AlignItems.Center,
            Gap = 8,
        };
        s[".slider-control"] = new S.Style
        {
            Padding = new S.EdgeValues(4, 4),
            Color = TextPrimary,
            Cursor = S.CursorType.Pointer,
        };

        // ── Swatch ───────────────────────────────────────────────
        s[".swatch"] = new S.Style
        {
            Width = S.StyleValue.Px(56),
            Height = S.StyleValue.Px(56),
            BorderRadius = 8,
            BorderWidth = 2,
            BorderColor = Outline,
        };
        s[".swatch-well"] = new S.Style
        {
            Background = BgSunken,
            BorderRadius = 10,
            Padding = new S.EdgeValues(3),
            AlignItems = S.AlignItems.Center,
            AlignSelf = S.AlignSelf.Center,
        };
        s[".swatch-col"] = new S.Style
        {
            AlignItems = S.AlignItems.Center,
            JustifyContent = S.JustifyContent.Center,
            Gap = 6,
            FlexGrow = 1,
        };

        // ── Colour wheel ─────────────────────────────────────────
        s[".wheel-wrap"] = new S.Style
        {
            Position = S.PositionType.Relative,
            Width = S.StyleValue.Px(BrushPanel.WheelPx),
            Height = S.StyleValue.Px(BrushPanel.WheelPx),
            FlexShrink = 0,
            Cursor = S.CursorType.Pointer,
        };
        s[".wheel"] = new S.Style
        {
            Width = S.StyleValue.Px(BrushPanel.WheelPx),
            Height = S.StyleValue.Px(BrushPanel.WheelPx),
            BorderRadius = BrushPanel.WheelPx / 2f,
        };
        s[".wheel-marker"] = new S.Style
        {
            Position = S.PositionType.Absolute,
            Width = S.StyleValue.Px(BrushPanel.MarkerPx),
            Height = S.StyleValue.Px(BrushPanel.MarkerPx),
            BorderRadius = BrushPanel.MarkerPx / 2f,
            BorderWidth = 2,
            BorderColor = Outline,

            // hard dark ring outside the white one, so the marker survives on a white centre
            BoxShadow = new S.BoxShadow { Blur = 0, Spread = 1.5f, Color = "rgba(0,0,0,0.9)" },

            // draw order comes from depth, not sibling order, so the wheel image would cover this
            ZIndex = 50,
        };

        // ── Value strip ──────────────────────────────────────────
        s[".strip-wrap"] = new S.Style
        {
            Height = S.StyleValue.Px(22),
            BorderRadius = 6,
            BorderWidth = 2,
            BorderColor = Outline,
            Padding = new S.EdgeValues(2),
            Overflow = S.Overflow.Hidden,
            Cursor = S.CursorType.Pointer,
        };
        s[".strip"] = new S.Style { Width = S.StyleValue.Percent(100), Height = S.StyleValue.Percent(100) };

        // ── Brush preview ────────────────────────────────────────
        s[".preview-frame"] = new S.Style
        {
            Width = S.StyleValue.Px(52),
            Height = S.StyleValue.Px(52),
            BorderRadius = 8,
            BorderWidth = 2,
            BorderColor = Outline,
            Padding = new S.EdgeValues(2),
            FlexShrink = 0,
        };
        s[".preview"] = new S.Style
        {
            Width = S.StyleValue.Percent(100),
            Height = S.StyleValue.Percent(100),
            BorderRadius = 6,
        };

        // ── Brush presets ────────────────────────────────────────
        s[".brush-strip"] = new S.Style
        {
            FlexDirection = S.FlexDirection.Row,
            Gap = 8,
            Padding = new S.EdgeValues(2, 0),
        };
        s[".brush-tile"] = new S.Style
        {
            Width = S.StyleValue.Px(44),
            Height = S.StyleValue.Px(44),
            BorderRadius = 8,
            BorderWidth = 2,
            BorderColor = OutlineSoft,
            Padding = new S.EdgeValues(2),
            FlexShrink = 0,
            Cursor = S.CursorType.Pointer,
            Hover = new S.Style { BorderColor = Outline },
        };
        s[".brush-tile-active"] = new S.Style
        {
            BorderColor = Accent,
            BorderWidth = 3,
        };
        s[".brush-tile-img"] = new S.Style
        {
            Width = S.StyleValue.Percent(100),
            Height = S.StyleValue.Percent(100),
            BorderRadius = 5,
        };
        s[".brush-add"] = new S.Style
        {
            Width = S.StyleValue.Px(44),
            Height = S.StyleValue.Px(44),
            BorderRadius = 8,
            BorderWidth = 2,
            BorderColor = OutlineSoft,
            Background = BgSunken,
            Color = TextPrimary,
            FontSize = 22,
            FontWeight = 700,
            AlignItems = S.AlignItems.Center,
            JustifyContent = S.JustifyContent.Center,
            TextAlign = S.TextAlign.Center,
            Padding = new S.EdgeValues(0),
            FlexShrink = 0,
            Cursor = S.CursorType.Pointer,
            Hover = new S.Style { BorderColor = Outline },
        };

        // ── Keybinds ─────────────────────────────────────────────
        s[".keybind-row"] = new S.Style
        {
            FlexDirection = S.FlexDirection.Row,
            AlignItems = S.AlignItems.Center,
            Gap = 8,
        };
        s[".keybind-key"] = new S.Style
        {
            Background = BgSunken,
            BorderRadius = 4,
            BorderWidth = 2,
            BorderColor = OutlineSoft,
            Padding = new S.EdgeValues(3, 7),
            FlexShrink = 0,
        };
        s[".keybind-key-text"] = new S.Style { FontSize = 11, FontWeight = 700, Color = TextPrimary };
        s[".keybind-action"] = new S.Style { FontSize = 12, Color = TextMuted, FlexGrow = 1 };

        // ── Buttons ──────────────────────────────────────────────
        s[".btn"] = new S.Style
        {
            Padding = new S.EdgeValues(7, 14),
            BorderRadius = 8,
            BorderWidth = 2,
            FontSize = 13,
            FontWeight = 700,
            AlignItems = S.AlignItems.Center,
            JustifyContent = S.JustifyContent.Center,
            Cursor = S.CursorType.Pointer,
            Transitions = new[] { new S.Transition { Property = "background", Duration = 0.15f, Easing = S.EasingType.Ease } },
        };
        s[".btn-accent"] = new S.Style
        {
            Background = Accent,
            Color = OnAccent,
            BorderColor = Outline,
            Hover = new S.Style { Background = "#b6ff8a" },
        };
        s[".btn-busy"] = new S.Style
        {
            Background = BgSunken,
            Color = TextMuted,
            BorderColor = OutlineSoft,
        };

        // ── Footer ───────────────────────────────────────────────
        s[".footer"] = new S.Style { Padding = new S.EdgeValues(7, 14) };
        s[".footer-text"] = new S.Style { FontSize = 11, Color = TextMuted };

        ReactUI.UI.RegisterStyles(s);
    }
}
