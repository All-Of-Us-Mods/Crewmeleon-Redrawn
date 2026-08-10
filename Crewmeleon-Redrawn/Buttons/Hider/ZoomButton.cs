using Crewmeleon_Redrawn.Components;
using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Roles;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Crewmeleon_Redrawn.Buttons.Hider;

public class ZoomButton : CustomActionButton
{
    protected override void OnClick()
    {
        ZoomCameraController.Instance.ToggleDisplay(!ZoomCameraController.Instance.IsActive);
    }

    public override bool CanUse()
    {
        return PlayerControl.LocalPlayer.HasModifier<PaintingModifier>();
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole;
    }

    public override string Name => "Zoom";

    public override float Cooldown => 1;

    public override LoadableAsset<Sprite> Sprite => MiraAssets.Cog;
}