using Crewmeleon_Redrawn.Components;
using HarmonyLib;
using Reactor.Utilities;
using UnityEngine;

namespace Crewmeleon_Redrawn.Patches;

[HarmonyPatch(typeof(PlayerControl))]
public static class PlayerControlPatch
{
    [HarmonyPatch(nameof(PlayerControl.Start))]
    [HarmonyPostfix]
    public static void PlayerControlStart(PlayerControl __instance)
    {
        Logger<CrewmeleonRedrawnPlugin>.Instance.LogInfo("Loaded");
        var newBody = new GameObject("PaintablePlayer")
        {
            transform =
            {
                parent = __instance.transform,
                localPosition = Vector3.zero,
                localScale = new Vector3(0.5f, 0.5f, 0)
            },
            layer = __instance.gameObject.layer,
        };
        
        var _playerCanvas = newBody.AddComponent<PlayerCanvasComponent>();
        _playerCanvas.Player = __instance;
    }
}