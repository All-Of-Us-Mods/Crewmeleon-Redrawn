using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Roles;
using CrewmeleonRedrawn.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace CrewmeleonRedrawn.Networking;

public static class TimerRpc
{
    private const float MinimumTauntVoteDistance = 3f;

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
            if (TryGetHiderMajority(out var position))
                SoundUtilities.PlayAtPosition(tauntSfx, position, 0.1f);
        }
        else
        {
            Error("Cannot update taunt timer state because Crewmeleon gamemode is not initialized.");
        }
    }

    private static bool TryGetHiderMajority(out Vector2 position)
    {
        position = default;

        var camera = Camera.main;
        var localPlayer = PlayerControl.LocalPlayer;
        if (!camera && !localPlayer) return false;

        var listenerPosition = camera
            ? (Vector2)camera!.transform.position
            : localPlayer!.GetTruePosition();
        var directionVotes = Vector2.zero;
        var weightedDistance = 0f;
        var totalWeight = 0f;
        var closestOffset = Vector2.zero;
        var closestDistance = float.MaxValue;
        var voteCount = 0;

        foreach (var hider in Helpers.GetAlivePlayers().Where(x => x.Data.Role is HiderRole))
        {
            var offset = hider.GetTruePosition() - listenerPosition;
            if (offset.sqrMagnitude <= 0f) continue;

            var distance = offset.magnitude;
            var weight = 1f / Mathf.Max(distance, MinimumTauntVoteDistance);
            directionVotes += offset.normalized * weight;
            weightedDistance += distance * weight;
            totalWeight += weight;
            voteCount++;

            if (distance >= closestDistance) continue;
            closestOffset = offset;
            closestDistance = distance;
        }

        if (voteCount == 0)
        {
            position = listenerPosition;
            return true;
        }

        var majorityDirection = directionVotes.sqrMagnitude > 0.0001f
            ? directionVotes.normalized
            : closestOffset.normalized;
        position = listenerPosition + majorityDirection * (weightedDistance / totalWeight);
        return true;
    }
}