using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
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
        if (ChameleonGameModeManager.Instance == null 
            || ChameleonGameModeManager.Instance.Timer.CurrentStage is TimerStage.Revelation) return;
        
        Coroutines.Start(CoShoot(shooter, position, splatterColor, splatterSize));
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
    
    private static IEnumerator CoShoot(PlayerControl shooter, Vector2 pos, Color32 splatterColor, float splatterSize)
    {
        if (!shooter.GetPlayerShotgun(out var shotgun))
        {
            Error($"{shooter.Data.PlayerName} attempted to shoot but shotgun is null.");
            yield break;
        }
        
        // ReSharper disable once Unity.PreferNonAllocApi
        // pre-sizing array is too much of a headache to get correctly, unity already sizes the array with the specific amount of colliders hit
        // player also has a large ass collider anyways (its a trigger collider), but we can use that as the full hitbox instead of expanding it
        var hitPlayerColliders = Physics2D.OverlapCircleAll(pos, 0.0001f, Constants.LivingPlayersOnlyMask);

        var killedPlayers = 0;
        foreach (var playerCollider in hitPlayerColliders)
        {
            if (killedPlayers >= ChameleonOptions.Gameplay.ShotgunKillsPerShot) break;
            var victim = playerCollider.GetComponent<PlayerControl>();
            if (!victim || !victim.Data) continue;
            if (victim.Data.Role is SeekerRole) continue;

            shooter.CustomMurder(victim, MurderResultFlags.Succeeded, teleportMurderer: false);
            SplatterComponent.CreateSplatter(pos, Palette.PlayerColors[victim.cosmetics.ColorId], splatterSize);
            killedPlayers++;
        }

        SoundUtilities.PlayAtPosition(CrewmeleonAssets.ShotgunFireSound.LoadAsset(), shooter.GetTruePosition(), 0.5f);

        if (shooter.AmOwner)
        {
            Coroutines.Start(HudManager.Instance.PlayerCam.CoShakeScreen(0.5f, 1).WrapToManaged());   
        }


        yield return shotgun!.CoFlashMuzzle();
    }
}