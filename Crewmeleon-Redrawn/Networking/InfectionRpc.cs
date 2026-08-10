using AmongUs.GameOptions;
using Crewmeleon_Redrawn.GameMode;
using Crewmeleon_Redrawn.Roles;
using MiraAPI.GameModes;
using MiraAPI.Roles;
using Reactor.Networking.Attributes;

namespace Crewmeleon_Redrawn.Networking;

public static class InfectionRpc
{
    [MethodRpc((uint)RPCCalls.Infect)]
    public static void RpcInfect(PlayerControl target)
    {
        var role = (RoleTypes)RoleId.Get<SeekerRole>();
        target.StartCoroutine(target.CoSetRole(role, true));
        (CustomGameModeManager.ActiveMode as ChameleonGameMode)!.NotifyOfDeath(target, infected: true);
    }
}