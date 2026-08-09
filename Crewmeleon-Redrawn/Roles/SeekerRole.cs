using MiraAPI.Roles;
using UnityEngine;

namespace Crewmeleon_Redrawn.Roles;

public class SeekerRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Seeker";

    public string RoleDescription => "Find the hiders at all costs!";

    public string RoleLongDescription => RoleDescription;

    public Color RoleColor => Palette.ImpostorRed;

    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.TaskHint
    };
}