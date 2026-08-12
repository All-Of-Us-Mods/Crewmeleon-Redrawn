using Crewmeleon_Redrawn.GameMode;
using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Roles;
using Crewmeleon_Redrawn.Utilities;
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

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole
            && !PlayerControl.LocalPlayer.HasModifier<PaintingModifier>()
            && (ChameleonGameMode.Instance is { CurrentStage: not TimerStage.Revelation } || CustomButtonUtilities.IsInPractice());
    }

    protected override void OnClick()
    {
        if (SpectatingModifier.GetSpectateTargets().Count > 1)
            PlayerControl.LocalPlayer.RpcAddModifier<SpectatingModifier>();
    }
}
