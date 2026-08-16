using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;

namespace CrewmeleonRedrawn.Networking;

public static class TimerRpc
{
    [MethodRpc((uint)CrewmeleonRpc.UpdateTimerState)]
    public static void RpcUpdateTimerState(this PlayerControl source, TimerStage stage)
    {
        if (!source.IsHost())
        {
            Error("Only host can update the timer state.");
            return;
        }

        if (ChameleonGameModeManager.Instance)
        {
            Info($"Host updated timer stage to {stage.ToString()}");
            ChameleonGameModeManager.Instance!.Timer.SetStage(stage);
        }
        else
        {
            Error("Cannot update timer state because Crewmeleon gamemode is not initialized.");
        }
    }
    
    [MethodRpc((uint)CrewmeleonRpc.SyncTimer)]
    public static void RpcSyncTimer(this PlayerControl source, TimerStage stage, float timeLeft)
    {
        if (!source.IsHost())
        {
            Error("Only host can sync the timer.");
            return;
        }

        if (ChameleonGameModeManager.Instance)
            ChameleonGameModeManager.Instance!.Timer.SyncRemaining(stage, timeLeft);
    }

    [MethodRpc((uint)CrewmeleonRpc.SyncTauntTimer)]
    public static void RpcUpdateTauntTimer(this PlayerControl source)
    {
        if (!source.IsHost())
        {
            Error("Only host can update the taunt timer state.");
            return;
        }

        if (!OptionGroupSingleton<TauntingOptions>.Instance.TauntingEnabled)
        {
            Error("Tried to update taunt timer but taunting is disabled.");
            return;
        }

        if (ChameleonGameModeManager.Instance)
        {
            Info($"Host updated taunt timer");
            ChameleonGameModeManager.Instance!.TauntTimer.ResetTimer();

            var tauntSfx = CrewmeleonAssets.AutomatedTauntSound.LoadAsset();
            foreach (var playerControl in Helpers.GetAlivePlayers().Where(x => !x.AmOwner))
                SoundUtilities.PlayAtPosition(tauntSfx, playerControl.GetTruePosition(), 0.1f);
        }
        else
        {
            Error("Cannot update taunt timer state because Crewmeleon gamemode is not initialized.");
        }
    }
}