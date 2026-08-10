using UnityEngine;

namespace Crewmeleon_Redrawn.Components;

public readonly struct PaintStroke(Vector2[] pixels, Color32 color)
{
    public Vector2[] Pixels => pixels;
    public Color32 Color => color;
}