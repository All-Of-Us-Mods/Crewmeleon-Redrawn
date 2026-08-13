using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using System.Collections;
using CrewmeleonRedrawn.Components;
using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Modifiers;
using CrewmeleonRedrawn.Roles;
using CrewmeleonRedrawn.Utilities;
using UnityEngine;

namespace CrewmeleonRedrawn.Buttons.Hider;

public class PickColorButton : CustomActionButton
{
    public const KeyCode PickKey = KeyCode.Space;

    public override string Name => "Pick";
    public override float Cooldown => 0;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Cog;

    public bool IsPicking { get; private set; }

    private static Texture2D? Sampler;

    private readonly ColorPickPreview preview = new();

    private bool shadowWasEnabled;
    private bool startedWithKey;

    public override bool CanUse() => !IsPicking;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole
            && PlayerControl.LocalPlayer.HasModifier<PaintingModifier>()
            && (ChameleonGameMode.Instance is { CurrentStage: not TimerStage.Revelation } || CustomButtonUtilities.IsInPractice());
    }

    protected override void OnClick()
    {
        BeginPick();
    }


    /// <summary>key picks sample on release, button picks wait for a click</summary>
    public bool ShouldCommitPick()
    {
        if (!IsPicking) return false;

        return startedWithKey
            ? Input.GetKeyUp(PickKey)
            : Pointer.SelectCommitted();
    }

    public IEnumerator CoPickColor(PlayerCanvasComponent canvas)
    {
        if (!Pointer.TryGetPosition(out var pointer))
        {
            StopPicking();
            RestoreShadow();
            yield break;
        }

        StopPicking();

        yield return new WaitForEndOfFrame();

        BrushStore.Local.SetFromColor(ReadScreenPixel(pointer));
        
        RestoreShadow();
    }

    private IEnumerator CoPreview()
    {
        while (IsPicking)
        {
            yield return new WaitForEndOfFrame();

            if (!IsPicking) break;

            // ensure shadow gets restored if they stop painting
            if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.HasModifier<PaintingModifier>())
            {
                StopPicking();
                RestoreShadow();
                break;
            }

            if (Pointer.TryGetPosition(out var pointer))
                preview.Show(pointer, ReadScreenPixel(pointer));
            else
                preview.Hide();
        }

        preview.Hide();
    }

    private void StopPicking()
    {
        IsPicking = false;
        preview.Hide();
    }

    private static Color ReadScreenPixel(Vector2 screenPosition)
    {
        var x = Mathf.Clamp((int) screenPosition.x, 0, Screen.width - 1);
        var y = Mathf.Clamp((int) screenPosition.y, 0, Screen.height - 1);

        Sampler ??= new Texture2D(1, 1, TextureFormat.RGB24, false);
        Sampler.ReadPixels(new Rect(x, y, 1, 1), 0, 0, false);
        Sampler.Apply();

        return Sampler.GetPixel(0, 0);
    }

    public void BeginPick(bool fromKey = false)
    {
        if (IsPicking) return;

        IsPicking = true;
        startedWithKey = fromKey;

        // the shadow overlay tints the whole screen so sampling through it comes back darker
        // than whats actually on the canvas
        // TODO: rework colour picker to ignore shadowquad without disabling
        var shadow = HudManager.Instance?.ShadowQuad;
        if (shadow)
        {
            shadowWasEnabled = shadow!.enabled;
            shadow.enabled = false;
        }

        if (ShipStatus.Instance.Type == ShipStatus.MapType.Fungle)
        {
            var tint = ShipStatus.Instance.transform.Find("Backgrounds/Base/OverlayTint");
            var jungleShadow = ShipStatus.Instance.transform.Find("FungleJungleShadow");
            jungleShadow.gameObject.SetActive(false);
            tint.gameObject.SetActive(false);
        }

        Coroutines.Start(CoPreview());
    }

    private void RestoreShadow()
    {
        var shadow = HudManager.Instance?.ShadowQuad;
        if (shadow) shadow!.enabled = shadowWasEnabled;

        if (ShipStatus.Instance.Type != ShipStatus.MapType.Fungle) return;

        var tint = ShipStatus.Instance.transform.Find("Backgrounds/Base/OverlayTint");
        var jungleShadow = ShipStatus.Instance.transform.Find("FungleJungleShadow");
        jungleShadow.gameObject.SetActive(true);
        tint.gameObject.SetActive(true);
    }
}
