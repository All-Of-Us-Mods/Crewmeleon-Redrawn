using CrewmeleonRedrawn.GameMode;
using MiraAPI.GameModes;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;

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

        if (CustomGameModeManager.ActiveMode is ChameleonGameMode mode)
        {
            Logger<CrewmeleonRedrawnPlugin>.Instance.LogInfo($"Host updated timer stage to {stage.ToString()}");
            mode.Timer.SetStage(stage);
        }
        else
        {
            Logger<CrewmeleonRedrawnPlugin>.Instance.LogError("Cannot update timer state because gamemode is not Crewmeleon.");
        }
    }
}