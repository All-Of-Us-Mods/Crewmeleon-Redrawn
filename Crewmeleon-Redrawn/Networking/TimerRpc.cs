using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;

namespace CrewmeleonRedrawn.Networking;

public static class TimerRpc
{
    [MethodRpc((uint)CrewmeleonRpc.UpdateTimerState)]
    public static void RpcUpdateTimerState(this PlayerControl source, TimerStage stage)
    {
        if (!source.IsHost())
        {
            Logger<CrewmeleonRedrawnPlugin>.Instance.LogError("Only host can update the timer state.");
            return;
        }

        if (ChameleonGameModeManager.Instance)
        {
            Logger<CrewmeleonRedrawnPlugin>.Instance.LogInfo($"Host updated timer stage to {stage.ToString()}");
            ChameleonGameModeManager.Instance!.Timer.SetStage(stage);
        }
        else
        {
            Logger<CrewmeleonRedrawnPlugin>.Instance.LogError("Cannot update timer state because Crewmeleon gamemode is not initialized.");
        }
    }
    
    [MethodRpc((uint)CrewmeleonRpc.SyncTauntTimer)]
    public static void RpcUpdateTauntTimer(this PlayerControl source)
    {
        if (!source.IsHost())
        {
            Logger<CrewmeleonRedrawnPlugin>.Instance.LogError("Only host can update the taunt timer state.");
            return;
        }

        if (!OptionGroupSingleton<TauntingOptions>.Instance.TauntingEnabled)
        {
            Logger<CrewmeleonRedrawnPlugin>.Instance.LogError("Tried to update taunt timer but taunting is disabled.");
            return;
        }

        if (ChameleonGameModeManager.Instance)
        {
            Logger<CrewmeleonRedrawnPlugin>.Instance.LogInfo($"Host updated taunt timer");
            ChameleonGameModeManager.Instance!.TauntTimer.ResetTimer();

            var tauntSfx = GameManagerCreator.Instance.HideAndSeekManagerPrefab.FinalHideAlertSFX;
            foreach (var playerControl in Helpers.GetAlivePlayers().Where(x => !x.AmOwner))
                SoundUtilities.PlayAtPosition(tauntSfx, playerControl.GetTruePosition(), 0.1f);
        }
        else
        {
            Logger<CrewmeleonRedrawnPlugin>.Instance.LogError("Cannot update taunt timer state because Crewmeleon gamemode is not initialized.");
        }
    }
}