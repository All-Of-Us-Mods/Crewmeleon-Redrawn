using System.Collections;
using Crewmeleon_Redrawn.Components;
using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Roles;
using Crewmeleon_Redrawn.Utilities;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace Crewmeleon_Redrawn.Buttons.Hider;

public class PickColorButton : CustomActionButton
{
    public const KeyCode PickKey = KeyCode.Space;

    public bool IsPicking { get; private set; }

    private readonly ColorPickPreview _preview = new();

    private static Texture2D? _sampler;

    private bool shadowWasEnabled;
    private bool startedWithKey;

    /// <summary>
    /// A key-driven pick is momentary — it samples the moment the key comes back up. A pick
    /// started from the button waits for a click instead.
    /// </summary>
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

        // only now — restoring before the read would put the shadow back into the sampled frame
        RestoreShadow();
    }

    private IEnumerator CoPreview()
    {
        while (IsPicking)
        {
            yield return new WaitForEndOfFrame();

            if (!IsPicking) break;

            // leaving paint mode mid-pick would otherwise strand the shadow off
            if (!PlayerControl.LocalPlayer || !PlayerControl.LocalPlayer.HasModifier<PaintingModifier>())
            {
                StopPicking();
                RestoreShadow();
                break;
            }

            if (Pointer.TryGetPosition(out var pointer))
                _preview.Show(pointer, ReadScreenPixel(pointer));
            else
                _preview.Hide();
        }

        _preview.Hide();
    }

    private void StopPicking()
    {
        IsPicking = false;
        _preview.Hide();
    }

    private static Color ReadScreenPixel(Vector2 screenPosition)
    {
        var x = Mathf.Clamp((int) screenPosition.x, 0, Screen.width - 1);
        var y = Mathf.Clamp((int) screenPosition.y, 0, Screen.height - 1);

        _sampler ??= new Texture2D(1, 1, TextureFormat.RGB24, false);
        _sampler.ReadPixels(new Rect(x, y, 1, 1), 0, 0, false);
        _sampler.Apply();

        return _sampler.GetPixel(0, 0);
    }

    protected override void OnClick()
    {
        BeginPick();
    }

    public void BeginPick(bool fromKey = false)
    {
        if (IsPicking) return;

        IsPicking = true;
        startedWithKey = fromKey;

        // the shadow overlay tints the whole screen, so sampling through it returns colours
        // darker than what is actually on the canvas
        // TODO: remove shadowquad completely?
        var shadow = HudManager.Instance?.ShadowQuad;
        if (shadow)
        {
            shadowWasEnabled = shadow!.enabled;
            shadow.enabled = false;
        }

        Coroutines.Start(CoPreview());
    }

    private void RestoreShadow()
    {
        var shadow = HudManager.Instance?.ShadowQuad;
        if (shadow) shadow!.enabled = shadowWasEnabled;
    }

    public override bool CanUse()
    {
        return !IsPicking;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole && PlayerControl.LocalPlayer.HasModifier<PaintingModifier>();
    }

    public override string Name => "Pick Color";
    public override float Cooldown => 0;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Cog;
}
