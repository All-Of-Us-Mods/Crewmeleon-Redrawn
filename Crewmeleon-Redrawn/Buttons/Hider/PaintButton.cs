using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Modifiers;
using CrewmeleonRedrawn.Roles;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace CrewmeleonRedrawn.Buttons.Hider;

public class PaintButton : CustomActionButton
{
    private const string PaintText = "Paint";
    private const string StopPaintingText = "Close";

    public override ButtonLocation Location =>
        CrewmeleonRedrawnPlugin.IsMobile ? ButtonLocation.BottomRight : ButtonLocation.BottomLeft;

    public override string Name => PaintText;
    public override float Cooldown => 1;
    public override LoadableAsset<Sprite> Sprite => CrewmeleonAssets.PaintButton;

    public override bool CanUse()
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole
            && !PlayerControl.LocalPlayer.HasModifier<SpectatingModifier>()
            && (ChameleonGameModeManager.Instance is { CurrentStage: not TimerStage.Revelation } || 
                CustomButtonUtilities.IsInPractice());
    }

    protected override void OnClick()
    {
        if (PlayerControl.LocalPlayer.HasModifier<PaintingModifier>())
        {
            PlayerControl.LocalPlayer.RpcRemoveModifier<PaintingModifier>();
            OverrideName(PaintText);
        }
        else
        {
            PlayerControl.LocalPlayer.RpcAddModifier<PaintingModifier>();
            OverrideName(StopPaintingText);
        }
    }
}