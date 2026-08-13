using System.Collections;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using Reactor.Utilities;

namespace CrewmeleonRedrawn.Events;

/**
 * fix for ChameleonGameMode.Instance is not null / ChameleonGameMode.Instance.CurrentStage in button Enabled
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