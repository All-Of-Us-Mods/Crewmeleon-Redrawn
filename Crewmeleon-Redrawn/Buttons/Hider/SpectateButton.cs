using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Roles;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Crewmeleon_Redrawn.Buttons.Hider;

public class SpectateButton : CustomActionButton
{
    public override string Name => "Spectate";
    public override float Cooldown => 5;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Cog;

    public override bool CanUse() => true;
    public override bool Enabled(RoleBehaviour? role) => role is HiderRole;

    protected override void OnClick()
    {
        if (SpectatingModifier.GetSpectateTargets().Count > 1)
            PlayerControl.LocalPlayer.RpcAddModifier<SpectatingModifier>();
    }
}