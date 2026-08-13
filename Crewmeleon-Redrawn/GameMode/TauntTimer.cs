using MiraAPI.Utilities;
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

    public void Begin()
    {
        // create the taunt timer bar and label if taunting is enabled
        if (!ChameleonOptions.Taunting.TauntingEnabled)
            return;

        maxTime = ChameleonOptions.Taunting.TauntCooldown.Value;
        timeLeft = maxTime;

        tauntBar = TimerBarFactory.Create(HudManager.Instance, Color.yellow, 0.75f, "NEXT TAUNT", out _);
        tauntBar.transform.localScale *= 0.7f;
    }

    // update the taunt timer bar and perform automatic taunt
    public void Update()
    {
        if (!ChameleonOptions.Taunting.TauntingEnabled)
            return;

        timeLeft -= Time.deltaTime;

        if (tauntBar is not null && tauntBar)
            tauntBar.UpdateTimer(timeLeft, maxTime);

        if (timeLeft > 0)
            return;

        timeLeft = maxTime;

        var tauntSfx = GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideCountdownSFX;
        foreach (var playerControl in Helpers.GetAlivePlayers().Where(x => !x.AmOwner))
            AudioSource.PlayClipAtPoint(tauntSfx, playerControl.GetTruePosition(), 0.1f);
    }

    public void End()
    {
        tauntBar?.gameObject.Destroy();
    }
}
