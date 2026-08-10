using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Roles;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Crewmeleon_Redrawn.Buttons.Hider;

public class PaintButton : CustomActionButton
{
    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.HasModifier<PaintingModifier>())
        {
            OverrideName("Paint");
            PlayerControl.LocalPlayer.RpcRemoveModifier<PaintingModifier>();
        }
        else
        {
            OverrideName("Stop");
            PlayerControl.LocalPlayer.RpcAddModifier<PaintingModifier>();
        }
    }

    public override bool CanUse()
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole;
    }

    public override string Name => "Paint";

    public override float Cooldown => 1;

    public override LoadableAsset<Sprite> Sprite => MiraAssets.Cog;
}