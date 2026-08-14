using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Modifiers;
using CrewmeleonRedrawn.Roles;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace CrewmeleonRedrawn.Buttons.Hider;

public class SpectateButton : CustomActionButton
{
    public override string Name => "Spectate";
    public override float Cooldown => 5;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Cog;

    public override ButtonLocation Location =>
        CrewmeleonRedrawnPlugin.IsMobile ? ButtonLocation.BottomRight : ButtonLocation.BottomLeft;

    public override bool CanUse()
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole
               && !PlayerControl.LocalPlayer.HasModifier<PaintingModifier>()
               && (ChameleonGameModeManager.Instance is { CurrentStage: not TimerStage.Revelation } ||
                   CustomButtonUtilities.IsInPractice());
    }

    protected override void OnClick()
    {
        if (SpectatingModifier.GetSpectateTargets().Count > 1)
            PlayerControl.LocalPlayer.RpcAddModifier<SpectatingModifier>();
    }
}