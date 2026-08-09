using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Roles;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Crewmeleon_Redrawn.Buttons.Hider;

public class SpectateButton : CustomActionButton
{
    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.AddModifier<SpectatingModifier>();
    }

    public override bool CanUse()
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole;
    }

    public override string Name => "Spectate";

    public override float Cooldown => 1;

    public override LoadableAsset<Sprite> Sprite => MiraAssets.Cog;
}