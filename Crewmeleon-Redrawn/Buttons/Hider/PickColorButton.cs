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
    public bool IsPicking { get; private set; }

    private readonly ColorPickPreview _preview = new();

    private static Texture2D? _sampler;

    public bool ShouldCommitPick()
    {
        return IsPicking && Pointer.SelectCommitted();
    }

    public IEnumerator CoPickColor(PlayerCanvasComponent canvas)
    {
        if (!Pointer.TryGetPosition(out var pointer))
        {
            StopPicking();
            yield break;
        }

        StopPicking();

        yield return new WaitForEndOfFrame();

        BrushStore.Local.SetFromColor(ReadScreenPixel(pointer));
    }

    private IEnumerator CoPreview()
    {
        while (IsPicking)
        {
            yield return new WaitForEndOfFrame();

            if (!IsPicking) break;

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

    public void BeginPick()
    {
        if (IsPicking) return;

        IsPicking = true;
        Coroutines.Start(CoPreview());
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
