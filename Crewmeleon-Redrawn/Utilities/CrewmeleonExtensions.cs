using CrewmeleonRedrawn.Components;
using System.Diagnostics.CodeAnalysis;

namespace CrewmeleonRedrawn.Utilities;

public static class CrewmeleonExtensions
{
    public static bool GetPlayerCanvas(this PlayerControl player, [NotNullWhen(true)] out PlayerCanvasComponent? canvas)
    {
        canvas = player.GetComponentInChildren<PlayerCanvasComponent>(true);
        return canvas;
    }
    
    public static bool GetPlayerShotgun(this PlayerControl player, [NotNullWhen(true)] out ShotgunComponent? shotgun)
    {
        shotgun = player.GetComponentInChildren<ShotgunComponent>(true);
        return shotgun;
    }

    public static void DisableMovement(this PlayerControl player)
    {
        player.moveable = false;
        player.NetTransform.Halt();
    }
}