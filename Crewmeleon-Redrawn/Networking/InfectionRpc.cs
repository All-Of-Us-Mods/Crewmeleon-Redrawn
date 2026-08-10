using AmongUs.GameOptions;
using Crewmeleon_Redrawn.GameMode;
using Crewmeleon_Redrawn.Roles;
using HarmonyLib;
using MiraAPI.GameModes;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;
using Object = UnityEngine.Object;

namespace Crewmeleon_Redrawn.Networking;

public static class InfectionRpc
{
    [MethodRpc((uint)RPCCalls.Infect)]
    public static void RpcInfect(PlayerControl target)
    {
        var seekerRole = RoleId.Get<SeekerRole>();
        ChangeRole(target, seekerRole);
        (CustomGameModeManager.ActiveMode as ChameleonGameMode)!.NotifyOfDeath(target, infected: true);
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
}