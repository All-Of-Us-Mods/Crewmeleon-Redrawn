using Crewmeleon_Redrawn.Components;
using Crewmeleon_Redrawn.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Crewmeleon_Redrawn.Roles;

public class HiderRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Chameleon";

    public string RoleDescription => "Camouflage to blend in with the map!";

    public string RoleLongDescription => RoleDescription;

    public Color RoleColor => Palette.CrewmateRoleBlue;

    public ModdedRoleTeams Team => ModdedRoleTeams.Crewmate;

    public CustomRoleConfiguration Configuration => new(this)
    {
        RoleHintType = RoleHintType.None
    };

    private static readonly LayerMask[] DisabledColliders =
    [
        LayerMask.NameToLayer("ShortObjects"),
        LayerMask.NameToLayer("Objects")
    ];

    private PlayerCanvasComponent _playerCanvas;

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (player.GetPlayerCanvas(out var canvas))
        {
            canvas!.Enable();
        }

        if (!player.AmOwner || !OptionGroupSingleton<GameplayOptions>.Instance.HideOnObjects.Value) return;
        foreach (var collider in ShipStatus.Instance.GetComponentsInChildren<Collider2D>().Where(x => DisabledColliders.Contains(x.gameObject.layer)))
        {
            if (collider.transform.parent.TryGetComponent<PlainDoor>(out _) || (collider.transform.TryGetComponent<IUsable>(out _) && !collider.transform.TryGetComponent<Console>(out _))) continue;

            collider.enabled = false;
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