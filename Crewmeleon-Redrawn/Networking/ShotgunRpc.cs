using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Crewmeleon_Redrawn.Components;
using Crewmeleon_Redrawn.Utilities;
using MiraAPI.Networking;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Crewmeleon_Redrawn.Networking;

public static class ShotgunRpc
{
    [MethodRpc((uint)CrewmeleonRpc.SyncShotgun)]
    public static void RpcSyncShotgun(this PlayerControl player, int zRot)
    {
        if (!player.GetPlayerShotgun(out var shotgun)) return;
        shotgun!.ZRotation = zRot;
    }

    [MethodRpc((uint)CrewmeleonRpc.ShootShotgun)]
    public static void RpcShootShotgun(this PlayerControl shooter, Vector2 position, Color32 splatterColor, float splatterSize)
    {
        Coroutines.Start(CoShoot(shooter, position, splatterColor, splatterSize));
    }
    
    private static IEnumerator CoShoot(PlayerControl shooter, Vector2 pos, Color32 splatterColor, float splatterSize)
    {
        if (!shooter.GetPlayerShotgun(out var shotgun)) yield break;

        var shot = Physics2D.OverlapCircle(pos, 0.5f, Constants.LivingPlayersOnlyMask);
        PlayerControl? plr = null;
        if (shot && shot.gameObject.TryGetComponent(out plr))
        {
            if (plr == shooter) yield break;

            shooter.CustomMurder(plr!, MurderResultFlags.Succeeded, teleportMurderer: false);
        }

        AudioSource.PlayClipAtPoint(CrewmeleonAssets.ShotgunFireSound.LoadAsset(), shooter.GetTruePosition(), 0.5f);

        if (shooter.AmOwner)
        {
            Coroutines.Start(HudManager.Instance.PlayerCam.CoShakeScreen(0.5f, 1).WrapToManaged());   
        }

        SplatterComponent.CreateSplatter(pos, plr ? Palette.PlayerColors[plr!.cosmetics.ColorId] : splatterColor, splatterSize);

        yield return shotgun!.CoFlashMuzzle();
    }
}