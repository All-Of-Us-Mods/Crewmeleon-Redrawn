using System.Collections;
using CrewmeleonRedrawn.Utilities;
using CrewmeleonRedrawn.Modifiers;
using CrewmeleonRedrawn.Networking;
using MiraAPI.GameModes;
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

    private HideAndSeekTimerBar? timerBar;
    private TextMeshPro? stageLabel;

    private float timeLeft;
    private float maxTime;
    private bool paused = true;
    private string StageText => CurrentStage.ToString().ToUpperInvariant();

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
                timeLeft = maxTime = OptionGroupSingleton<GameplayOptions>.Instance.HideTime.Value;
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

    public void Update()
    {
        if (timerBar is null || !timerBar || paused)
            return;

        timeLeft -= Time.deltaTime;
        timerBar.UpdateTimer(timeLeft, maxTime);
        
        if (CurrentStage == TimerStage.Hiding)
            HoldSeekerStill();

        if (timeLeft > 0) return;
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

    public float GetTimeLeft() => timeLeft;

    private void HoldSeekerStill()
    {
        if (!ChameleonGameMode.AmImpostor || PlayerControl.LocalPlayer.MyPhysics.Speed == 0)
            return;

        PlayerControl.LocalPlayer.DisableMovement();
    }

    private void SetupSeekingStage()
    {
        timeLeft = maxTime = ChameleonOptions.Gameplay.SeekTime.Value;

        SetBarColor(Palette.ImpostorRed);

        SoundManager.Instance.PlaySound(GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideAlertSFX, false, 1);
        
        var gamemode = CustomGameModeManager.ActiveMode as ChameleonGameMode;
        gamemode?.TauntTimer.Begin();
        
        if (ChameleonGameMode.AmImpostor && PlayerControl.LocalPlayer.MyPhysics.Speed <= 0)
        {
            PlayerControl.LocalPlayer.moveable = true;
        }
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

        var gamemode = CustomGameModeManager.ActiveMode as ChameleonGameMode;
        gamemode?.TauntTimer.End();
        
        // give each hider a revelation period
        if (ChameleonOptions.Gameplay.RevelationTimePerPlayer.Value > 0 && hiders.Count > 0)
        {
            maxTime = hiders.Count * ChameleonOptions.Gameplay.RevelationTimePerPlayer;
            timeLeft = maxTime;

            SetBarColor(Color.yellow);

            Coroutines.Start(CoReveal(hiders));
            return;
        }

        maxTime = 0;
        timeLeft = 0;
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

        PlayerControl.LocalPlayer.moveable = false;
        PlayerControl.LocalPlayer.NetTransform.Halt();
        PlayerControl.LocalPlayer.MyPhysics.Speed = 0;
        HudManager.Instance.ShadowQuad.enabled = false;

        foreach (var player in players)
        {
            HudManager.Instance.PlayerCam.Target = player;
            yield return new WaitForSeconds(ChameleonOptions.Gameplay.RevelationTimePerPlayer.Value);
        }
    }
}
