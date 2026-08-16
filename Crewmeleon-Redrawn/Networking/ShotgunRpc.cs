using System.Collections;
using CrewmeleonRedrawn.Utilities;
using CrewmeleonRedrawn.Components;
using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Roles;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace CrewmeleonRedrawn.Networking;

public static class ShotgunRpc
{
    [MethodRpc((uint)CrewmeleonRpc.SyncShotgun)]
    public static void RpcSyncShotgun(this PlayerControl shooter, int zRot)
    {
        if (!shooter.GetPlayerShotgun(out var shotgun))
        {
            Error($"Attempted to sync shotgun for {shooter.Data.PlayerName} but shotgun is null.");
            return;
        }

        shotgun!.ZRotation = zRot;
    }

    [MethodRpc((uint)CrewmeleonRpc.ShootShotgun)]
    public static void RpcShootShotgun(this PlayerControl shooter, Vector2 position, Color32 splatterColor, float splatterSize)
    {
        if ((ChameleonGameModeManager.Instance == null 
            || ChameleonGameModeManager.Instance.Timer.CurrentStage is not TimerStage.Seeking) 
            && !CustomButtonUtilities.IsInPractice()) return;
        
        Coroutines.Start(CoShoot(shooter, position));
        if (splatterSize > 0f)
        {
            SplatterComponent.CreateSplatter(position, splatterColor, splatterSize);
        }
    }

    [MethodRpc((uint)CrewmeleonRpc.SplatKill)]
    public static void RpcSplatKill(this PlayerControl shooter, byte[] victimIds, float splatterSize)
    {
        if ((ChameleonGameModeManager.Instance == null 
             || ChameleonGameModeManager.Instance.Timer.CurrentStage is not TimerStage.Seeking) 
            && !CustomButtonUtilities.IsInPractice()) return;
        
        foreach (var victimId in victimIds)
        {
            var victim = GameData.Instance.GetPlayerById(victimId)?.Object;
            if (!victim)
            {
                Error($"{shooter.Data.PlayerName} shot player {victimId} but they could not be found.");
                continue;
            }

            if (victim.Data.IsDead || victim.Data.Role is SeekerRole) continue;

            if (ChameleonOptions.Gameplay.InfectionMode) ChameleonInfection.Infect(victim);
            else shooter.CustomMurder(victim, MurderResultFlags.Succeeded, teleportMurderer: false);
            SplatterComponent.CreateSplatter(victim.GetTruePosition(), Palette.PlayerColors[victim.cosmetics.ColorId], splatterSize);
        }
    }
    
    [MethodRpc((uint)CrewmeleonRpc.ToggleShotgun)]
    public static void RpcToggleShotgun(this PlayerControl shooter, bool visible)
    {
        if (!shooter.GetPlayerShotgun(out var shotgun))
        {
            Error($"Attempted to toggle shotgun for {shooter.Data.PlayerName} but shotgun is null.");
            return;
        }
        
        shotgun.gameObject.SetActive(visible);
    }
    
    private static IEnumerator CoShoot(PlayerControl shooter, Vector2 targetPosition)
    {
        if (!shooter.GetPlayerShotgun(out var shotgun))
        {
            Error($"{shooter.Data.PlayerName} attempted to shoot but shotgun is null.");
            yield break;
        }

        SoundUtilities.PlayAtPosition(CrewmeleonAssets.ShotgunFireSound.LoadAsset(), shooter.GetTruePosition(), 0.5f);

        if (shooter.AmOwner)
            Coroutines.Start(CoCameraRecoil(shooter.GetTruePosition(), targetPosition, 0.55f, 0.25f, 0.5f, 1f));

        yield return shotgun!.CoFlashMuzzle();
    }

    private static IEnumerator CoCameraRecoil(Vector2 origin, Vector2 target, float recoilDistance, float recoilDuration, float shakeDuration, float shakeSeverity)
    {
        var followerCamera = HudManager.Instance.PlayerCam;
        if (!followerCamera) yield break;

        var recoilDirection = origin - target;
        var recoil = recoilDirection.sqrMagnitude > 0f
            ? recoilDirection.normalized * recoilDistance
            : Vector2.zero;
        var wait = new WaitForFixedUpdate();
        var appliedOffset = Vector2.zero;
        var elapsed = 0f;
        var duration = Mathf.Max(recoilDuration, shakeDuration);

        while (elapsed < duration && followerCamera)
        {
            var recoilStrength = elapsed < recoilDuration
                ? 1f - Mathf.SmoothStep(0f, 1f, elapsed / recoilDuration)
                : 0f;
            var shakeStrength = elapsed < shakeDuration
                ? 1f - elapsed / shakeDuration
                : 0f;
            var nextOffset = recoil * recoilStrength
                + UnityEngine.Random.insideUnitCircle * shakeStrength * shakeSeverity;
            followerCamera.Offset += nextOffset - appliedOffset;
            appliedOffset = nextOffset;

            elapsed += Time.fixedDeltaTime;
            yield return wait;
        }

        if (followerCamera) followerCamera.Offset -= appliedOffset;
    }
}