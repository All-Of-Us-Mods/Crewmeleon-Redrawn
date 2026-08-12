using Crewmeleon_Redrawn.GameMode;
using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Roles;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Crewmeleon_Redrawn.Buttons.Hider;

public class PaintButton : CustomActionButton
{
    private const string PaintText = "Paint";
    private const string StopPaintingText = "Close";

    public override string Name => PaintText;
    public override float Cooldown => 1;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Cog;

    public override bool CanUse() => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole
            && ChameleonGameMode.Instance is not null
            && ChameleonGameMode.Instance.CurrentStage != TimerStage.Revelation;
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
