using S = ReactUI.Style;

namespace Crewmeleon_Redrawn.UI;

/// <summary>
/// Stylesheet for the Crewmeleon panels. Accent is the game mode's own green so the UI reads as
/// part of the mod rather than the ReactUI defaults.
/// </summary>
public static class CrewmeleonStyles
{
    public static readonly S.UIColor BgMain = "#23272a";
    public static readonly S.UIColor BgBar = "#1a1d1f";
    public static readonly S.UIColor BgPanel = "#2b3033";
    public static readonly S.UIColor BgSunken = "#171a1c";

    public static readonly S.UIColor Accent = "#6fbf3f";
    public static readonly S.UIColor AccentText = "#96ff5a";
    public static readonly S.UIColor OnAccent = "#10170a";

    public static readonly S.UIColor TextPrimary = "#dfe4e0";
    public static readonly S.UIColor TextMuted = "#8b968f";

    public static readonly S.UIColor BtnBg = "#353b3e";
    public static readonly S.UIColor Divider = "rgba(255,255,255,0.08)";
    public static readonly S.UIColor AccentHover = "rgba(111,191,63,0.30)";
    public static readonly S.UIColor Hairline = "rgba(255,255,255,0.06)";

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

        // ── Panel structure ──────────────────────────────────────
        s[".panel-outer"] = new S.Style
        {
            Position = S.PositionType.Absolute,
            Width = 272,
            BorderRadius = 14,
            BoxShadow = new S.BoxShadow { OffsetY = 8, Blur = 32, Color = "rgba(0,0,0,0.6)" },
        };
        s[".panel-inner"] = new S.Style
        {
            Overflow = S.Overflow.Hidden,
            Background = BgMain,
            BorderRadii = (14, 14, 0, 0),
            BorderColor = Hairline,
            BorderWidth = 1,
        };
        s[".panel-body"] = new S.Style { Padding = new S.EdgeValues(12), Gap = 10 };

        // ── Title bar ────────────────────────────────────────────
        s[".title-bar"] = new S.Style
        {
            FlexDirection = S.FlexDirection.Row,
            JustifyContent = S.JustifyContent.SpaceBetween,
            AlignItems = S.AlignItems.Center,
            Background = BgBar,
            Padding = new S.EdgeValues(9, 14),
            BorderRadii = (14, 14, 0, 0),
            Gap = 10,
        };
        s[".title-text"] = new S.Style { FontSize = 15, FontWeight = 700, Color = AccentText };
        s[".title-hint"] = new S.Style { FontSize = 11, Color = TextMuted };

        // ── Section ──────────────────────────────────────────────
        s[".section"] = new S.Style
        {
            Background = BgPanel,
            BorderRadius = 8,
            Padding = new S.EdgeValues(10, 12),
            Gap = 7,
        };
        s[".section-title"] = new S.Style { FontSize = 11, FontWeight = 700, Color = TextMuted };

        // ── Text ─────────────────────────────────────────────────
        s[".text-label"] = new S.Style { FontSize = 13, Color = TextPrimary, FlexShrink = 0 };
        s[".text-value"] = new S.Style
        {
            FontSize = 12,
            Color = TextMuted,
            Width = S.StyleValue.Px(38),
            TextAlign = S.TextAlign.Right,
        };

        // ── Sliders ──────────────────────────────────────────────
        s[".setting-row-slider"] = new S.Style
        {
            FlexDirection = S.FlexDirection.Row,
            JustifyContent = S.JustifyContent.SpaceBetween,
            AlignItems = S.AlignItems.Center,
            Gap = 10,
        };
        s[".slider-control"] = new S.Style
        {
            Padding = new S.EdgeValues(4, 6),
            Color = Accent,
            Cursor = S.CursorType.Pointer,
        };

        // ── Swatch / hue strip ───────────────────────────────────
        s[".swatch"] = new S.Style
        {
            Width = S.StyleValue.Px(38),
            Height = S.StyleValue.Px(38),
            BorderRadius = 8,
            BorderWidth = 1,
            BorderColor = "rgba(255,255,255,0.20)",
            FlexShrink = 0,
        };
        s[".swatch-well"] = new S.Style
        {
            Background = BgSunken,
            BorderRadius = 9,
            Padding = new S.EdgeValues(2),
            FlexShrink = 0,
        };
        s[".hex-text"] = new S.Style
        {
            FontSize = 11,
            FontWeight = 700,
            Color = TextMuted,
            TextAlign = S.TextAlign.Center,
        };

        // ── Colour wheel ─────────────────────────────────────────
        s[".wheel-wrap"] = new S.Style
        {
            Position = S.PositionType.Relative,
            Width = S.StyleValue.Px(BrushPanel.WheelPx),
            Height = S.StyleValue.Px(BrushPanel.WheelPx),
            FlexShrink = 0,
        };
        s[".wheel"] = new S.Style
        {
            Width = S.StyleValue.Px(BrushPanel.WheelPx),
            Height = S.StyleValue.Px(BrushPanel.WheelPx),
            BorderRadius = BrushPanel.WheelPx / 2f,
            Cursor = S.CursorType.Pointer,
        };
        s[".wheel-marker"] = new S.Style
        {
            Position = S.PositionType.Absolute,
            Width = S.StyleValue.Px(BrushPanel.MarkerPx),
            Height = S.StyleValue.Px(BrushPanel.MarkerPx),
            BorderRadius = BrushPanel.MarkerPx / 2f,
            BorderWidth = 2,
            BorderColor = "#ffffff",
            BoxShadow = new S.BoxShadow { Blur = 3, Color = "rgba(0,0,0,0.55)" },
        };

        // ── Buttons ──────────────────────────────────────────────
        s[".btn"] = new S.Style
        {
            Padding = new S.EdgeValues(7, 14),
            BorderRadius = 6,
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
            Hover = new S.Style { Background = AccentText },
        };
        s[".btn-busy"] = new S.Style
        {
            Background = BtnBg,
            Color = TextMuted,
        };

        // ── Divider / footer ─────────────────────────────────────
        s[".divider"] = new S.Style { Height = S.StyleValue.Px(1), Background = Divider };
        s[".footer"] = new S.Style
        {
            Background = BgBar,
            Padding = new S.EdgeValues(7, 14),
            BorderRadii = (0, 0, 14, 14),
        };
        s[".footer-text"] = new S.Style { FontSize = 11, Color = TextMuted };
        s[".footer-anchor"] = new S.Style { Margin = new S.EdgeValues(-1, 0, 0, 0) };

        ReactUI.UI.RegisterStyles(s);
    }
}
