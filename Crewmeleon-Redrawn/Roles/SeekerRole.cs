using CrewmeleonRedrawn.Buttons.Seeker;
using MiraAPI.Hud;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using UnityEngine;

namespace CrewmeleonRedrawn.Roles;

public class SeekerRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Seeker";
    public string RoleDescription => "Catch the chameleons at all costs.";
    public string RoleLongDescription => RoleDescription;

    public Color RoleColor => Palette.ImpostorRed;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.None,
        CanUseVent = false,
        CanUseSabotage = false
    };
    
    public Func<bool> VisibleInSettings => () => false;
    
    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        player.cosmetics.transform.localPosition = new Vector3(0, 0, -0.5f);
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (!targetPlayer.AmOwner) return;

        if (CustomButtonSingleton<ShotgunButton>.Instance.Equipped)
        {
            CustomButtonSingleton<ShotgunButton>.Instance.ToggleShotgun();
        }
    }
}