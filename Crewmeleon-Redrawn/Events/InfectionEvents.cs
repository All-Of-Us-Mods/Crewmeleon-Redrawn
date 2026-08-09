using AmongUs.GameOptions;
using Crewmeleon_Redrawn.Roles;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;

namespace Crewmeleon_Redrawn.Events;

public static class InfectionEvents
{
    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        if (!CustomGameModeManager.IsActiveGameMode<ChameleonGameMode>())
        {
            return;
        }
        if (!OptionGroupSingleton<GameplayOptions>.Instance.InfectionMode)
        {
            return;
        }

        var player = @event.Target;
        @event.Cancel();
        player.RpcSetRole((RoleTypes)RoleId.Get<SeekerRole>());
        (CustomGameModeManager.ActiveMode as ChameleonGameMode)!.NotifyOfDeath(player, true);
    }
}