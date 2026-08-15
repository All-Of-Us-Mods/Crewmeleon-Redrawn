using System.Collections;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Modifiers;
using CrewmeleonRedrawn.Roles;
using CrewmeleonRedrawn.States;
using HarmonyLib;
using MiraAPI.Modifiers;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Object = UnityEngine.Object;

namespace CrewmeleonRedrawn.GameMode;

public static class ChameleonInfection
{
    public static void Infect(PlayerControl target)
    {
        var seekerRole = RoleId.Get<SeekerRole>();
        ChangeRole(target, seekerRole);
        target.StartCoroutine(PlaySeekerAnimation(target));
        if (target.HasModifier<SpectatingModifier>()) target.GetModifierComponent().RemoveModifier<SpectatingModifier>();
        if (target.HasModifier<PaintingModifier>()) target.GetModifierComponent().RemoveModifier<PaintingModifier>();
        ChameleonGameModeManager.Instance!.NotifyOfDeath(target, infected: true);
        if (target.AmOwner)
        {
            Coroutines.Start(HudManager.Instance.PlayerCam.CoShakeScreen(0.5f, 1).WrapToManaged());
        }
    }
    
    private static void ChangeRole(PlayerControl player, ushort newRoleType)
    {
        player.roleAssigned = false;

        var data = player.Data;

        if (data.Role)
        {
            data.Role.Deinitialize(player);
        }

        var newRole = RoleManager.Instance.GetRole((RoleTypes)newRoleType);
        var roleBehaviour = Object.Instantiate(newRole, data.gameObject.transform);

        roleBehaviour.Initialize(player);

        if (player.AmOwner && HudManager.Instance)
        {
            HudManager.Instance.SetHudActive(player, roleBehaviour, true);

            if (MeetingHud.Instance || ExileController.Instance)
            {
                HudManager.Instance.SetHudActive(player, roleBehaviour, false);
            }
        }

        player.Data.Role = roleBehaviour;
        player.Data.RoleType = roleBehaviour.Role;

        if (!roleBehaviour.IsDead)
        {
            player.Data.RoleWhenAlive = new Il2CppSystem.Nullable<RoleTypes>(roleBehaviour.Role);
        }

        roleBehaviour.AdjustTasks(player);
        
        player.Data.Role.SpawnTaskHeader(player);
        
        player.MyPhysics.SetBodyType(player.BodyType);
        
        PlayerNameColor.Set(player);
        if (player.AmOwner)
        {
            PlayerControl.AllPlayerControls.ToArray().Do(PlayerNameColor.Set);
        }
    }

    private static IEnumerator PlaySeekerAnimation(PlayerControl player)
    {
        using var movementBlock = player.BlockMovement("seeker spawn animation");
        yield return player.MyPhysics.CoAnimateCustom(HudManager.Instance.IntroPrefab.HnSSeekerSpawnAnim);
    }
}