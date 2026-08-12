
using Crewmeleon_Redrawn.Components;
using HarmonyLib;
using MiraAPI.GameOptions;
using UnityEngine;

namespace Crewmeleon_Redrawn.Patches;

[HarmonyPatch(typeof(PlayerControl))]
public static class PlayerControlPatch
{
    [HarmonyPatch(nameof(PlayerControl.Start))]
    [HarmonyPostfix]
    public static void PlayerControlStart(PlayerControl __instance)
    {
        var zValue = OptionGroupSingleton<GameplayOptions>.Instance.HideOnObjects
                     || OptionGroupSingleton<GameplayOptions>.Instance.AlwaysOnTop
            ? -0.3f
            : 0f;
        
        var newBody = new GameObject("PaintablePlayer")
        {
            transform =
            {
                parent = __instance.transform,
                localPosition = new Vector3(0, 0, zValue),
                localScale = new Vector3(0.5f, 0.5f, 1)
            },
            layer = __instance.gameObject.layer,
        };
        
        var _playerCanvas = newBody.AddComponent<PlayerCanvasComponent>();
        _playerCanvas.Player = __instance;
        ShotgunComponent.CreateShotgun(__instance);
    }
}