using Crewmeleon_Redrawn.GameMode;
using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Roles;
using Crewmeleon_Redrawn.Utilities;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Crewmeleon_Redrawn.Buttons.Hider;

public class LockMovementButton : CustomActionButton
{
    private const string LockText = "Lock";
    private const string UnlockText = "Unlock";

    public override string Name => LockText;
    public override float Cooldown => 0;
    public override LoadableAsset<Sprite> Sprite => CrewmeleonAssets.LockButton;

    public bool IsLocked { get; private set; } = true;

    public override bool CanUse() => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole
               && !PlayerControl.LocalPlayer.HasModifier<PaintingModifier>()
               && (ChameleonGameMode.Instance is { CurrentStage: not TimerStage.Revelation } || CustomButtonUtilities.IsInPractice());
    }

    protected override void OnClick()
    {
        IsLocked = !IsLocked;

        PlayerControl.LocalPlayer.moveable = !IsLocked;
        PlayerControl.LocalPlayer.NetTransform.Halt();
        
        OverrideName(IsLocked ? UnlockText : LockText);
        OverrideSprite(IsLocked ? CrewmeleonAssets.UnlockButton.LoadAsset() : CrewmeleonAssets.LockButton.LoadAsset());
    }
}