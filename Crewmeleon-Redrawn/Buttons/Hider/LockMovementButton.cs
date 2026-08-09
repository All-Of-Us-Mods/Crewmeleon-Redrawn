using Crewmeleon_Redrawn.Roles;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Crewmeleon_Redrawn.Buttons.Hider;

public class LockMovementButton : CustomActionButton
{
    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.moveable = !PlayerControl.LocalPlayer.moveable;
        PlayerControl.LocalPlayer.NetTransform.Halt();
        
        OverrideName(PlayerControl.LocalPlayer.CanMove ? "Lock Movement" : "Unlock Movement");
        OverrideSprite(PlayerControl.LocalPlayer.CanMove ? Assets.LockButton.LoadAsset() : Assets.UnlockButton.LoadAsset());
    }

    public override bool CanUse()
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole;
    }

    public override string Name => "Lock Movement";

    public override float Cooldown => 0;

    public override LoadableAsset<Sprite> Sprite => Assets.LockButton;
}