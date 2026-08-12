using Crewmeleon_Redrawn.Components;
using UnityEngine;

namespace Crewmeleon_Redrawn.Utilities;

public static class CrewmeleonExtensions
{
    public static bool GetPlayerCanvas(this PlayerControl player, out PlayerCanvasComponent? canvas)
    {
        var playerCanvas = player.GetComponentInChildren<PlayerCanvasComponent>(true);
        if (playerCanvas == null)
        {
            canvas = null;
            return false;
        }

        canvas = playerCanvas;
        return true;
    }
}