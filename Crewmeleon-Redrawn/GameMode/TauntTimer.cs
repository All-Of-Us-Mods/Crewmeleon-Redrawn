using CrewmeleonRedrawn.Networking;
using CrewmeleonRedrawn.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace CrewmeleonRedrawn.GameMode;

/// <summary>
/// Drives the taunt cooldown bar and plays the periodic taunt sound.
/// </summary>
public class TauntTimer
{
    private HideAndSeekTimerBar? tauntBar;

    private float timeLeft;
    private float maxTime;
    private bool paused;

    public void Begin()
    {
        // create the taunt timer bar and label if taunting is enabled
        if (!ChameleonOptions.Taunting.TauntingEnabled)
            return;

        tauntBar = HudUtilities.CreateTimerBar(HudManager.Instance, Color.yellow, 0.75f, "NEXT TAUNT", out _);
        tauntBar.transform.localScale *= 0.7f;
        ResetTimer();
    }

    public void ResetTimer()
    {
        timeLeft = maxTime = ChameleonOptions.Taunting.TauntCooldown.Value;
        paused = false;
    }

    // update the taunt timer bar and perform automatic taunt
    public void Update()
    {
        if (!ChameleonOptions.Taunting.TauntingEnabled || paused)
            return;

        timeLeft -= Time.deltaTime;

        if (tauntBar is not null && tauntBar)
            tauntBar.UpdateTimer(timeLeft, maxTime);

        if (timeLeft > 0) return;
        paused = true;

        if (!AmongUsClient.Instance.AmHost) return;
        PlayerControl.LocalPlayer.RpcUpdateTauntTimer();
    }

    public void End()
    {
        tauntBar?.gameObject.Destroy();
    }
}
