using AmongUs.GameOptions;
using Crewmeleon_Redrawn.Roles;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Roles;

namespace Crewmeleon_Redrawn.Events;

public static class InfectionEvents
{
    [RegisterEvent]
    public static void BeforeMurderEventHandler(BeforeMurderEvent @event)
    {
        if (!OptionGroupSingleton<GameplayOptions>.Instance.InfectionMode)
        {
            return;
        }
        
        @event.Cancel();
        @event.Target.RpcSetRole((RoleTypes)RoleId.Get<SeekerRole>());
    }
}