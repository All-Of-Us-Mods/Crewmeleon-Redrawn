using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Networking;
using CrewmeleonRedrawn.Roles;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace CrewmeleonRedrawn.Buttons.Seeker;

public class ShotgunButton : CustomActionButton
{
    public bool Equipped;
    public override string Name => "Shotgun";
    public override float Cooldown => 1;
    public override MiraKeybind? Keybind => MiraGlobalKeybinds.PrimaryAbility;
    public override LoadableAsset<Sprite> Sprite => CrewmeleonAssets.ShotgunButton;
    public override ButtonLocation Location =>
        CrewmeleonRedrawnPlugin.IsMobile ? ButtonLocation.BottomRight : ButtonLocation.BottomLeft;

    public override bool CanUse()
    {
        return true;
    }

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is SeekerRole
               && (ChameleonGameModeManager.Instance is { CurrentStage: not TimerStage.Revelation } ||
                   CustomButtonUtilities.IsInPractice());
    }

    public override void CreateButton(Transform parent)
    {
        base.CreateButton(parent);
        Equipped = false;
    }

    protected override void OnClick() => ToggleShotgun();
    public void ToggleShotgun()
    {
        var hasShotgun = PlayerControl.LocalPlayer.GetPlayerShotgun(out _);
        if (!hasShotgun) return;

        Equipped = !Equipped;
        Cursor.SetCursor(Equipped ? CrewmeleonAssets.TargetSprite.LoadAsset().texture : null, new Vector2(512, 512),
            CursorMode.Auto);
        PlayerControl.LocalPlayer.RpcToggleShotgun(Equipped);
    }
}