using Crewmeleon_Redrawn.Components;
using HarmonyLib;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace Crewmeleon_Redrawn.Patches;

[HarmonyPatch(typeof(HudManager))]
public static class HudManagerPatch
{
    [HarmonyPatch(nameof(HudManager.Start))]
    [HarmonyPostfix]
    public static void HudManagerStart(HudManager __instance)
    {
        var mainCam = Camera.main!;
        var camObj = new GameObject("ZoomCamera");
        camObj.transform.SetParent(mainCam.transform, false);
        camObj.AddComponent<ZoomCameraController>();
    }
}