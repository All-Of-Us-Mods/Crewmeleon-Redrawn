using CrewmeleonRedrawn.Utilities;
using CrewmeleonRedrawn.Components;
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

    public Func<bool> VisibleInSettings => () => false;

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
        
        var layerMask = 0;
        foreach (var layer in DisabledColliders) layerMask |= 1 << layer;
        Player.Collider.excludeLayers = layerMask;
    }

    public override void Deinitialize(PlayerControl targetPlayer)
    {
        RoleBehaviourStubs.Deinitialize(this, targetPlayer);

        if(playerCanvas is not null && playerCanvas)
            playerCanvas.Disable();

        Player.Collider.excludeLayers = 0;
    }
}