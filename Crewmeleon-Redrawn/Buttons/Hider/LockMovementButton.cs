using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Roles;
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
    public override LoadableAsset<Sprite> Sprite => Assets.LockButton;

    public bool Locked { get; private set; } = true;

    public override bool CanUse() => true;

    public override bool Enabled(RoleBehaviour? role)
        => role is HiderRole && !PlayerControl.LocalPlayer.HasModifier<PaintingModifier>();

    protected override void OnClick()
    {
        Locked = !Locked;

        PlayerControl.LocalPlayer.moveable = !Locked;
        PlayerControl.LocalPlayer.NetTransform.Halt();
        
        OverrideName(Locked ? UnlockText : LockText);
        OverrideSprite(Locked ? Assets.UnlockButton.LoadAsset() : Assets.LockButton.LoadAsset());
    }
}