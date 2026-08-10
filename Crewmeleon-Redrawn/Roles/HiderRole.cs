using Crewmeleon_Redrawn.Components;
using Crewmeleon_Redrawn.Utilities;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
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
        RoleHintType = RoleHintType.None
    };
    
    private PlayerCanvasComponent _playerCanvas;

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (player.GetPlayerCanvas(out var canvas))
        {
            canvas!.Enable();
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if (targetPlayer.GetPlayerCanvas(out var canvas))
        {
            canvas!.Disable();
        }
    }
}