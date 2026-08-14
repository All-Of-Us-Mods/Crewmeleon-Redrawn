using System.Collections;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using Reactor.Utilities;

namespace CrewmeleonRedrawn.Events;

/**
 * this shouldn't be needed anymore, keeping it as a load bearing coconut 
 */
public static class RoundStartEventListener
{
    [RegisterEvent]
    public static void OnRoundStartEvent(RoundStartEvent @event)
    {
        RefreshButtonsDeferred();
    }

    private static void RefreshButtonsDeferred()
    {
        if (PlayerControl.LocalPlayer == null) return;

        Coroutines.Start(CoRefreshButtons());
    }

    private static IEnumerator CoRefreshButtons()
    {
        yield return null;
        CustomButtonUtilities.RefreshVisibility();
    }
}