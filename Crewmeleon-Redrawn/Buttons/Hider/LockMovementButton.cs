using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Modifiers;
using CrewmeleonRedrawn.Networking;
using CrewmeleonRedrawn.Roles;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace CrewmeleonRedrawn.Buttons.Hider;

public class LockMovementButton : CustomActionButton
{
    private const string LockText = "Lock";
    private const string UnlockText = "Unlock";
    
    public override string Name => LockText;
    public override float Cooldown => 0;
    
    public override ButtonLocation Location =>
        CrewmeleonRedrawnPlugin.IsMobile ? ButtonLocation.BottomRight : ButtonLocation.BottomLeft;
    
    public override LoadableAsset<Sprite> Sprite => CrewmeleonAssets.LockButton;
    public override MiraKeybind? Keybind => MiraGlobalKeybinds.TertiaryAbility;

    public bool IsLocked { get; private set; }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        IsLocked = false;
    }
    
    public override bool CanUse()
    {
        return true;
    }
    
    public override bool Enabled(RoleBehaviour? role)
    {
        return role is HiderRole
               && !PlayerControl.LocalPlayer.HasModifier<PaintingModifier>()
               && !PlayerControl.LocalPlayer.HasModifier<SpectatingModifier>()
               && (ChameleonGameModeManager.Instance is { CurrentStage: not TimerStage.Revelation } ||
                   CustomButtonUtilities.IsInPractice());
    }
    
    protected override void OnClick()
    {
        IsLocked = !IsLocked;

        if (IsLocked)
            PlayerControl.LocalPlayer.RpcResyncTransform(PlayerControl.LocalPlayer.cosmetics.FlipX);
        
        OverrideName(IsLocked ? UnlockText : LockText);
        OverrideSprite(IsLocked ? CrewmeleonAssets.UnlockButton.LoadAsset() : CrewmeleonAssets.LockButton.LoadAsset());
    }
}