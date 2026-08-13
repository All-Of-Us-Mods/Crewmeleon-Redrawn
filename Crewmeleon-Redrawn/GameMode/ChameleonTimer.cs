using System.Collections;
using AmongUs.GameOptions;
using CrewmeleonRedrawn.Modifiers;
using MiraAPI.GameModes;
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
    Hiding,
}

/// <summary>
/// Drives the hide/seek/revelation stage timer and its HUD bar.
/// </summary>
public class ChameleonTimer
{
    public TimerStage CurrentStage { get; private set; }

    public bool IsActive => timerBar is not null && timerBar;

    private HideAndSeekTimerBar? timerBar;
    private TextMeshPro? stageLabel;

    private float timeLeft;
    private float maxTime;

    private float defaultSeekerSpeed;

    private string StageText => CurrentStage.ToString().ToUpperInvariant();

    public void Begin(HudManager hud)
    {
        CurrentStage = TimerStage.Hiding;
        maxTime = ChameleonOptions.Gameplay.HideTime.Value;
        timeLeft = maxTime;

        timerBar = TimerBarFactory.Create(hud, Palette.CrewmateBlue, 0.35f, StageText, out stageLabel);
    }

    public void Update()
    {
        if (timerBar is null || !timerBar)
            return;

        timeLeft -= Time.deltaTime;
        timerBar.UpdateTimer(timeLeft, maxTime);

        // prevent the seekers from moving in the hiding stage
        if (CurrentStage == TimerStage.Hiding)
            HoldSeekerStill();

        // end the current stage if the timer has reached 0
        if (timeLeft <= 0)
            AdvanceStage();
    }

    private void HoldSeekerStill()
    {
        if (!ChameleonGameMode.AmImpostor || PlayerControl.LocalPlayer.MyPhysics.Speed == 0)
            return;

        defaultSeekerSpeed = PlayerControl.LocalPlayer.MyPhysics.Speed;

        // surely there's a better way?
        PlayerControl.LocalPlayer.moveable = false;
        PlayerControl.LocalPlayer.NetTransform.Halt();
        PlayerControl.LocalPlayer.MyPhysics.Speed = 0;
    }

    private void AdvanceStage()
    {
        if (CurrentStage == TimerStage.Hiding)
            OnHidingStageEnd();
        else if (CurrentStage == TimerStage.Seeking)
            OnSeekingStageEnd();
        else if (CurrentStage == TimerStage.Revelation)
            OnRevelationStageEnd();
        
        if (stageLabel is not null && stageLabel)
            stageLabel.text = StageText;
    }

    /// <summary>
    /// End the hiding stage and start the seeking stage.
    /// </summary>
    private void OnHidingStageEnd()
    {
        CurrentStage = TimerStage.Seeking;

        maxTime = ChameleonOptions.Gameplay.SeekTime.Value;
        timeLeft = maxTime;

        SetBarColor(Palette.ImpostorRed);

        SoundManager.Instance.PlaySound(GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideAlertSFX, false, 1);
        
        var gamemode = CustomGameModeManager.ActiveMode as ChameleonGameMode;
        gamemode?.TauntTimer.Begin();
        
        // allow the seeker to move when the seeking stage starts
        if (ChameleonGameMode.AmImpostor && PlayerControl.LocalPlayer.MyPhysics.Speed <= 0)
        {
            PlayerControl.LocalPlayer.moveable = true;
            PlayerControl.LocalPlayer.MyPhysics.Speed = defaultSeekerSpeed;
        }
    }

    /// <summary>
    /// End the seeking stage and start the revelation stage.
    /// </summary>
    private void OnSeekingStageEnd()
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

    /// <summary>
    /// End the revelation stage and end the game.
    /// </summary>
    private static void OnRevelationStageEnd()
    {
        if (!PlayerControl.LocalPlayer.IsHost())
            return;

        GameManager.Instance.RpcEndGame(GameOverReason.HideAndSeek_CrewmatesByTimer, false);
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
