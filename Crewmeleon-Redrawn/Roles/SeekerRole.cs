using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using UnityEngine;

namespace Crewmeleon_Redrawn.Roles;

public class SeekerRole : ImpostorRole, ICustomRole
{
    public string RoleName => "Seeker";

    public string RoleDescription => "Find the hiders at all costs!";

    public string RoleLongDescription => RoleDescription;

    public Color RoleColor => Palette.ImpostorRed;

    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.None,
        CanUseVent = false
    };
    
    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        player.cosmetics.currentBodySprite.BodySprite.transform.parent.localPosition = new Vector3(0, 0, -0.4f);
    }
}