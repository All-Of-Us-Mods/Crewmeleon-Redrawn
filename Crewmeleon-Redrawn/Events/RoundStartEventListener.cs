using CrewmeleonRedrawn.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;

namespace CrewmeleonRedrawn.Events;

/**
 * this shouldn't be needed anymore, keeping it as a load bearing coconut 
 */
public static class RoundStartEventListener
{
    [RegisterEvent]
    public static void OnRoundStartEvent(RoundStartEvent @event)
    {
        CustomButtonUtilities.RefreshVisibilityDeferred();
    }
}