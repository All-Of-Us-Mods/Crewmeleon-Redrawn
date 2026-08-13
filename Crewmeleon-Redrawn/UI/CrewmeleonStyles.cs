using UnityEngine;
using S = ReactUI.Style;

namespace CrewmeleonRedrawn.UI;

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

    public const float PanelWidthPercent = 24f;

    public const float ScreenGutter = 14f;

    private const float PanelPadding = 12f;
    private const float SectionBorder = 2f;
    private const float SectionPaddingX = 12f;
    private const float SectionPaddingY = 10f;

    private const float SliderThumbWidth = 14f;
    private const float SliderThumbHeight = 36f;
    private const float SliderThumbRadius = 2f;
    private const float SliderTrackHeight = 12f;
    private const float SliderPadding = 4f;

    /// <summary>desktop keeps the slider's width but halves how tall it stands</summary>
    private static float SliderScaleY => MobileUi.Active ? 1f : DesktopSliderScaleY;

    private static float ThumbHeight => SliderThumbHeight * SliderScaleY;

    public static float SliderThumbWidthPx =>
        Px(SliderThumbWidth * (MobileUi.Active ? 1f : DesktopThumbScaleX));
    public static float SliderThumbHeightPx => Px(ThumbHeight);
    public static float SliderThumbRadiusPx => Px(SliderThumbRadius);
    public static float SliderTrackHeightPx => Px(SliderTrackHeight * SliderScaleY);

    private const float DesignWidth = 272f;
    private const float DesignHeightSingle = 600f;
    private const float DesignHeightSplit = 350f;

    private const float ReferenceHeight = 1080f;
    private const float DesktopFontScale = 0.49f;
    private const float DesktopSliderScaleY = 0.5f;
    private const float DesktopThumbScaleX = 0.5f;

    private static float ValueStripHeight => MobileUi.Active ? 33f : 23f;

    public static float Scale { get; private set; } = 1f;

    public static float PanelTop { get; private set; }

    public static float PanelWidth { get; private set; }

    public static float LogicalHeight { get; private set; }

    /// <summary>the inset that centres a panel once layout has told us how tall it really is</summary>
    public static float CentreTop(float height) =>
        Mathf.Max(Mathf.Round((LogicalHeight - height) / 2f), ScreenGutter);

    /// <summary>what a section has left inside the panel once every border and padding is paid</summary>
    public static float SectionContentWidth =>
        PanelWidth - (OutlineWidth + Px(PanelPadding) + Px(SectionBorder) + Px(SectionPaddingX)) * 2f;

    public static float OutlineWidth => Px(3f);

    private static int lastScreenWidth;
    private static int lastScreenHeight;

    public static float Px(float value) => Scale == 1f ? value : Mathf.Round(value * Scale);

    /// <summary>desktop reads at arm's length, so it takes the whole sheet's text down a notch</summary>
    private static float FontScale => MobileUi.Active ? 1f : DesktopFontScale;

    private static float Fs(float value) => Px(value * FontScale);

    private static S.StyleValue Sv(float value) => S.StyleValue.Px(Px(value));

    /// <summary>for widths that exist only to fit a piece of text, so they track the font</summary>
    private static S.StyleValue TextWidth(float value) => S.StyleValue.Px(Fs(value));

    private static S.EdgeValues Edge(float all) => new(Px(all));

    private static S.EdgeValues Edge(float vertical, float horizontal) => new(Px(vertical), Px(horizontal));

    public static bool RefreshIfResolutionChanged()
    {
        if (Screen.width == lastScreenWidth && Screen.height == lastScreenHeight) return false;

        Register();
        return true;
    }

    private static void ComputeLayout()
    {
        var factor = Mathf.Max(Screen.height / ReferenceHeight, 0.5f);
        var logicalWidth = Screen.width / factor;

        LogicalHeight = Screen.height / factor;
        PanelWidth = Mathf.Round(logicalWidth * PanelWidthPercent / 100f);

        var availableHeight = LogicalHeight - ScreenGutter * 2f;
        var designHeight = MobileUi.Active ? DesignHeightSplit : DesignHeightSingle;

        Scale = Mathf.Clamp(Mathf.Min(PanelWidth / DesignWidth, availableHeight / designHeight), 0.5f, 4f);

        // only the opening position — the ticker re-centres on the measured height once laid out
        PanelTop = CentreTop(designHeight * Scale);
    }

    public static void Register()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        ComputeLayout();

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
        s[".items-top"] = new S.Style { AlignItems = S.AlignItems.FlexStart };
        s[".self-center"] = new S.Style { AlignSelf = S.AlignSelf.Center };
        s[".gap-6"] = new S.Style { Gap = Px(6) };
        s[".gap-10"] = new S.Style { Gap = Px(10) };

        // ── Panel ────────────────────────────────────────────────
        // each panel is its own root; a wrapper would cover the screen and eat every game click
        s[".panel"] = new S.Style
        {
            Position = S.PositionType.Absolute,
            Width = S.StyleValue.Percent(PanelWidthPercent),
            Background = BgMain,
            BorderRadius = Px(16),
            BorderWidth = OutlineWidth,
            BorderColor = Outline,
            Overflow = S.Overflow.Hidden,
        };
        s[".panel-body"] = new S.Style { Padding = Edge(PanelPadding), Gap = Px(10) };

        // ── Section ──────────────────────────────────────────────
        s[".section"] = new S.Style
        {
            Background = BgPanel,
            BorderRadius = Px(10),
            BorderWidth = Px(SectionBorder),
            BorderColor = OutlineSoft,
            Padding = Edge(SectionPaddingY, SectionPaddingX),
            Gap = Px(8),
        };
        s[".section-title"] = new S.Style { FontSize = Fs(22), FontWeight = 700, Color = TextMuted };

        // ── Text ─────────────────────────────────────────────────
        s[".text-label"] = new S.Style { FontSize = Fs(24), FontWeight = 700, Color = TextPrimary, FlexShrink = 0 };
        s[".slider-label"] = new S.Style { Width = TextWidth(120) };
        s[".text-value"] = new S.Style
        {
            FontSize = Fs(22),
            FontWeight = 700,
            Color = TextMuted,
            Width = TextWidth(68),
            FlexShrink = 0,
            TextAlign = S.TextAlign.Right,
        };

        // ── Sliders ──────────────────────────────────────────────
        s[".setting-row-slider"] = new S.Style
        {
            FlexDirection = S.FlexDirection.Row,
            JustifyContent = S.JustifyContent.SpaceBetween,
            AlignItems = S.AlignItems.Center,
            Gap = Px(8),
        };
        s[".slider-control"] = new S.Style
        {
            Height = Sv(ThumbHeight + SliderPadding * 2f),
            Padding = Edge(SliderPadding, SliderPadding),
            Color = TextPrimary,
            Cursor = S.CursorType.Pointer,
        };

        // ── Swatch ───────────────────────────────────────────────
        s[".swatch"] = new S.Style
        {
            Width = Sv(56),
            Height = Sv(56),
            BorderRadius = Px(8),
            BorderWidth = Px(2),
            BorderColor = Outline,
        };
        s[".swatch-well"] = new S.Style
        {
            Background = BgSunken,
            BorderRadius = Px(10),
            Padding = Edge(3),
            AlignItems = S.AlignItems.Center,
            AlignSelf = S.AlignSelf.Center,
        };
        s[".swatch-col"] = new S.Style
        {
            AlignItems = S.AlignItems.Stretch,
            Gap = Px(6),
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
            BorderWidth = Px(2),
            BorderColor = Outline,

            // hard dark ring outside the white one, so the marker survives on a white centre
            BoxShadow = new S.BoxShadow { Blur = 0, Spread = Px(1.5f), Color = "rgba(0,0,0,0.9)" },

            // draw order comes from depth, not sibling order, so the wheel image would cover this
            ZIndex = 50,
        };

        // ── Value strip ──────────────────────────────────────────
        s[".strip-wrap"] = new S.Style
        {
            Height = Sv(ValueStripHeight),
            BorderRadius = Px(6),
            BorderWidth = Px(2),
            BorderColor = Outline,
            Padding = Edge(2),
            Overflow = S.Overflow.Hidden,
            Cursor = S.CursorType.Pointer,
        };
        s[".strip-row"] = new S.Style
        {
            FlexDirection = S.FlexDirection.Row,
            Width = S.StyleValue.Percent(100),
            Height = S.StyleValue.Percent(100),
            AlignItems = S.AlignItems.Center,
        };
        s[".strip-spacer"] = new S.Style { FlexShrink = 0 };
        s[".strip-marker"] = new S.Style
        {
            Width = Sv(3),
            Height = S.StyleValue.Percent(100),
            Background = Outline,
            FlexShrink = 0,
        };

        // ── Keybinds ─────────────────────────────────────────────
        s[".keybind-row"] = new S.Style
        {
            FlexDirection = S.FlexDirection.Row,
            AlignItems = S.AlignItems.Center,
            Gap = Px(8),
        };
        s[".keybind-key"] = new S.Style
        {
            Background = BgSunken,
            BorderRadius = Px(4),
            BorderWidth = Px(2),
            BorderColor = OutlineSoft,
            Padding = Edge(3, 7),
            FlexShrink = 0,
        };
        s[".keybind-key-text"] = new S.Style { FontSize = Fs(15), FontWeight = 700, Color = TextPrimary };
        s[".keybind-action"] = new S.Style { FontSize = Fs(17), Color = TextMuted, FlexGrow = 1 };

        // ── Buttons ──────────────────────────────────────────────
        s[".btn"] = new S.Style
        {
            Padding = Edge(7, 14),
            BorderRadius = Px(8),
            BorderWidth = Px(2),
            FontSize = Fs(26),
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
        s[".btn-dark"] = new S.Style
        {
            Background = BgSunken,
            Color = TextPrimary,
            BorderColor = Outline,
            Hover = new S.Style { Background = BgPanel },
        };
        s[".btn-busy"] = new S.Style
        {
            Background = BgSunken,
            Color = TextMuted,
            BorderColor = OutlineSoft,
        };

        // ── Footer ───────────────────────────────────────────────
        s[".footer"] = new S.Style { Padding = Edge(7, 14) };
        s[".footer-text"] = new S.Style { FontSize = Fs(22), Color = TextMuted };

        ReactUI.UI.RegisterStyles(s);
    }
}
