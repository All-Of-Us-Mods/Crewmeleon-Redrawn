using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Networking;
using Crewmeleon_Redrawn.Roles;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Crewmeleon_Redrawn.Buttons.Hider;

public class TauntButton : CustomActionButton
{
    protected override void OnClick()
    {
        PlayerControl.LocalPlayer.RpcTaunt();
    }

    public override bool CanUse()
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole;
    }

    public override string Name => "Taunt";

    public override float Cooldown => 5;

    public override LoadableAsset<Sprite> Sprite => MiraAssets.Cog;
}