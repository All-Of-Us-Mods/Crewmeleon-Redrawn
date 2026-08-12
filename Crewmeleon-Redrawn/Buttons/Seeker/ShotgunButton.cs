using Crewmeleon_Redrawn.GameMode;
using Crewmeleon_Redrawn.Modifiers;
using Crewmeleon_Redrawn.Roles;
using Crewmeleon_Redrawn.Utilities;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace Crewmeleon_Redrawn.Buttons.Seeker;

public class ShotgunButton : CustomActionButton
{
    public override string Name => "Shotgun";
    public override float Cooldown => 1;
    public override LoadableAsset<Sprite> Sprite => MiraAssets.Cog;
    private bool _equipped;

    public override bool CanUse() => true;

    public override bool Enabled(RoleBehaviour? role)
    {
        return role is SeekerRole
               && (ChameleonGameMode.Instance is { CurrentStage: not TimerStage.Revelation } || CustomButtonUtilities.IsInPractice());
    }

    protected override void OnClick()
    {
        var hasShotgun = PlayerControl.LocalPlayer.GetPlayerShotgun(out var shotgun);
        if (!hasShotgun) return;
        
        _equipped = !_equipped;
        Cursor.SetCursor(_equipped ? Assets.TargetSprite.LoadAsset().texture : null, CursorMode.Auto);
        shotgun!.gameObject.SetActive(_equipped);
    }
}
