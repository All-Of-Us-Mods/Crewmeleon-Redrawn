using CrewmeleonRedrawn.Utilities;
using CrewmeleonRedrawn.Components;
using MiraAPI.GameOptions;
using MiraAPI.Patches.Stubs;
using MiraAPI.Roles;
using UnityEngine;
using CrewmeleonRedrawn.GameMode;

namespace CrewmeleonRedrawn.Roles;

public class HiderRole : CrewmateRole, ICustomRole
{
    public string RoleName => "Chameleon";
    public string RoleDescription => "Camouflage to blend in with the map.";
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

    private PlayerCanvasComponent? playerCanvas;

    public override void Initialize(PlayerControl player)
    {
        RoleBehaviourStubs.Initialize(this, player);

        if (player.GetPlayerCanvas(out var canvas))
        {
            canvas.Enable();
            playerCanvas = canvas;
        }

        if (!player.AmOwner || !ChameleonOptions.Gameplay.HideOnObjects.Value)
            return;

        foreach (var collider in ShipStatus.Instance.GetComponentsInChildren<Collider2D>().Where(x => DisabledColliders.Contains(x.gameObject.layer)))
        {
            if (collider.transform.parent.TryGetComponent<PlainDoor>(out _) 
                || (collider.transform.TryGetComponent<IUsable>(out _) 
                && !collider.transform.TryGetComponent<Console>(out _)))
                continue;

            collider.enabled = false;
        }
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if(playerCanvas is not null && playerCanvas)
            playerCanvas.Disable();
    }
}