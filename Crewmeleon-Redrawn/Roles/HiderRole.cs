using MiraAPI.Roles;
using UnityEngine;

namespace Crewmeleon_Redrawn.Roles;

public class HiderRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Hider";

    public string RoleDescription => "Draw to camouflage yourself on the map!";

    public string RoleLongDescription => RoleDescription;

    public Color RoleColor => Palette.CrewmateRoleBlue;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.TaskHint
    };
}