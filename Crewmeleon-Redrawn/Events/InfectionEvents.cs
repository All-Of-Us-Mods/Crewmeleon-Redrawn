using AmongUs.GameOptions;
using CrewmeleonRedrawn.Roles;
using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Networking;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.Roles;

namespace CrewmeleonRedrawn.Events;

public static class InfectionEvents
{
    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        if (!CustomGameModeManager.IsActiveGameMode<ChameleonGameMode>()
            || !OptionGroupSingleton<GameplayOptions>.Instance.InfectionMode)
        {
            return;
        }

        @event.Cancel();

        if (!AmongUsClient.Instance.AmHost) return;

        var player = @event.Target;
        InfectionRpc.RpcInfect(player);
    }
}