using System.Collections;
using Crewmeleon_Redrawn.Components;
using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Roles;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Crewmeleon_Redrawn.Buttons.Hider;

public class PickColorButton : CustomActionButton
{
    public bool WaitingForClick { get; private set; }

    public IEnumerator CoPickColor(PlayerCanvasComponent canvas)
    {
        var mouse = Input.mousePosition;
        yield return new WaitForEndOfFrame();

        var x = Mathf.Clamp((int)mouse.x, 0, Screen.width - 1);
        var y = Mathf.Clamp((int)mouse.y, 0, Screen.height - 1);

        var pixel = new Texture2D(1, 1, TextureFormat.RGB24, false);
        pixel.ReadPixels(new Rect(x, y, 1, 1), 0, 0, false);
        pixel.Apply();

        canvas.BrushColor = pixel.GetPixel(0, 0);
        pixel.Destroy();
        WaitingForClick = false;
    }

    protected override void OnClick()
    {
        WaitingForClick = true;
    }

    public override bool CanUse()
    {
        return PlayerControl.LocalPlayer.HasModifier<PaintingModifier>() && !WaitingForClick;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole;
    }

    public override string Name => "Pick Color";
    public override float Cooldown => 0;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Cog;
}