using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Networking;
using CrewmeleonRedrawn.Roles;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.Hud;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace CrewmeleonRedrawn.Buttons.Seeker;

public class ShotgunButton : CustomActionButton
{
    private bool _equipped;
    public override string Name => "Shotgun";
    public override float Cooldown => 1;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Cog;
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
        _equipped = false;
    }

    protected override void OnClick()
    {
        var hasShotgun = PlayerControl.LocalPlayer.GetPlayerShotgun(out var shotgun);
        if (!hasShotgun) return;

        _equipped = !_equipped;
        Cursor.SetCursor(_equipped ? CrewmeleonAssets.TargetSprite.LoadAsset().texture : null, new Vector2(512, 512),
            CursorMode.Auto);
        PlayerControl.LocalPlayer.RpcToggleShotgun(_equipped);
    }
}