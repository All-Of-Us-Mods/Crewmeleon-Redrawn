using CrewmeleonRedrawn.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.Events.Vanilla.Player;

namespace CrewmeleonRedrawn.Events;

/**
 * this shouldn't be needed anymore, keeping it as a load bearing coconut 
 */
public static class DeathEventListener
{
    [RegisterEvent]
    public static void OnDeath(PlayerDeathEvent @event)
    {
        @event.Player.moveable = true;
        CustomButtonUtilities.RefreshVisibilityDeferred();
    }
}