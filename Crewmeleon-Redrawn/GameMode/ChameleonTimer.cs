using System.Collections;
using CrewmeleonRedrawn.Utilities;
using CrewmeleonRedrawn.Modifiers;
using CrewmeleonRedrawn.Networking;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using Reactor.Utilities;
using TMPro;
using UnityEngine;

namespace CrewmeleonRedrawn.GameMode;

public enum TimerStage
{
    Revelation,
    Seeking,
    Hiding
}

/// <summary>
/// Drives the hide/seek/revelation stage timer and its HUD bar.
/// </summary>
public class ChameleonTimer
{
    public TimerStage CurrentStage { get; private set; } = TimerStage.Hiding;

    public bool IsActive => timerBar is not null && timerBar;

    private const float SyncInterval = 2f;
    private const float SyncTolerance = 0.5f;

    private HideAndSeekTimerBar? timerBar;
    private TextMeshPro? stageLabel;

    private float deadline;
    private float maxTime;
    private float nextSyncAt;
    private bool paused = true;
    private string StageText => CurrentStage.ToString().ToUpperInvariant();

    private float TimeLeft => Mathf.Max(0f, deadline - Time.realtimeSinceStartup);

    public void CreateTimer(HudManager hud)
    {
        timerBar = HudUtilities.CreateTimerBar(hud, Palette.CrewmateBlue, 0.35f, StageText, out stageLabel);
    }

    public void SetStage(TimerStage stage)
    {
        paused = false;
        CurrentStage = stage;

        switch (stage)
        {
            case TimerStage.Hiding:
                StartCountdown(OptionGroupSingleton<GameplayOptions>.Instance.HideTime.Value);
                break;
            case TimerStage.Seeking:
                SetupSeekingStage();
                break;
            case TimerStage.Revelation:
                SetupRevealStage();
                break;
        }

        if (stageLabel is not null && stageLabel)
            stageLabel.text = StageText;
    }

    public void SyncRemaining(TimerStage stage, float remaining)
    {
        if (stage != CurrentStage)
        {
            SetStage(stage);
        }

        if (Mathf.Abs(TimeLeft - remaining) <= SyncTolerance) return;

        deadline = Time.realtimeSinceStartup + remaining;
        maxTime = Mathf.Max(maxTime, remaining);
    }

    public void Update()
    {
        if (timerBar is null || !timerBar || paused)
            return;

        var timeLeft = TimeLeft;
        timerBar.UpdateTimer(timeLeft, maxTime);

        if (timeLeft > 0)
        {
            if (AmongUsClient.Instance.AmHost && Time.realtimeSinceStartup >= nextSyncAt)
            {
                nextSyncAt = Time.realtimeSinceStartup + SyncInterval;
                PlayerControl.LocalPlayer.RpcSyncTimer(CurrentStage, timeLeft);
            }

            return;
        }

        paused = true;

        if (!AmongUsClient.Instance.AmHost) return;
        switch (CurrentStage)
        {
            case TimerStage.Hiding:
                PlayerControl.LocalPlayer.RpcUpdateTimerState(TimerStage.Seeking);
                break;
            case TimerStage.Seeking:
                PlayerControl.LocalPlayer.RpcUpdateTimerState(TimerStage.Revelation);
                break; 
            case TimerStage.Revelation:
                GameManager.Instance.RpcEndGame(GameOverReason.HideAndSeek_CrewmatesByTimer, false);
                break;
        }
    }

    public float GetTimeLeft() => TimeLeft;

    private void StartCountdown(float duration)
    {
        maxTime = duration;
        deadline = Time.realtimeSinceStartup + duration;
        nextSyncAt = Time.realtimeSinceStartup + SyncInterval;
    }

    private void SetupSeekingStage()
    {
        StartCountdown(ChameleonOptions.Gameplay.SeekTime.Value);

        SetBarColor(Palette.ImpostorRed);

        SoundUtilities.Play(GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideAlertSFX);
        
        ChameleonGameModeManager.Instance?.TauntTimer.Begin();
    }

    private void SetupRevealStage()
    {
        CurrentStage = TimerStage.Revelation;

        var hiders = Helpers.GetAlivePlayers().Where(p => !p.Data.Role.IsImpostor).ToList();

        // forcefully close painting or spectating screen
        if(PlayerControl.LocalPlayer.HasModifier<PaintingModifier>()) 
            PlayerControl.LocalPlayer.RpcRemoveModifier<PaintingModifier>();

        if (PlayerControl.LocalPlayer.HasModifier<SpectatingModifier>())
            PlayerControl.LocalPlayer.RpcRemoveModifier<SpectatingModifier>();

        ChameleonGameModeManager.Instance?.TauntTimer.End();
        
        // give each hider a revelation period
        if (ChameleonOptions.Gameplay.RevelationTimePerPlayer.Value > 0 && hiders.Count > 0)
        {
            StartCountdown(hiders.Count * ChameleonOptions.Gameplay.RevelationTimePerPlayer);

            SetBarColor(Color.yellow);

            Coroutines.Start(CoReveal(hiders));
            return;
        }

        StartCountdown(0);
    }

    private void SetBarColor(Color color)
    {
        if (timerBar is not null && timerBar)
            timerBar.timerBarRenderer.material.SetColor("_Color", color);
    }

    private static IEnumerator CoReveal(List<PlayerControl> players)
    {
        if (PlayerControl.LocalPlayer.HasModifier<SpectatingModifier>())
            PlayerControl.LocalPlayer.RemoveModifier<SpectatingModifier>();

        HudManager.Instance.ShadowQuad.enabled = false;

        foreach (var player in players)
        {
            HudManager.Instance.PlayerCam.Target = player;
            yield return new WaitForSeconds(ChameleonOptions.Gameplay.RevelationTimePerPlayer.Value);
        }
    }
}
