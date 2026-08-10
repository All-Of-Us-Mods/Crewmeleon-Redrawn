using MiraAPI;
using UnityEngine;

namespace Crewmeleon_Redrawn.Utilities;

/// <summary>
/// Mouse on desktop, first touch on mobile.
/// </summary>
public static class Pointer
{
    public static bool IsMobile => MiraApiPlugin.IsMobile;

    public static bool TryGetPosition(out Vector2 position)
    {
        if (!IsMobile)
        {
            position = Input.mousePosition;
            return true;
        }

        if (Input.touchCount == 0)
        {
            position = default;
            return false;
        }

        position = Input.GetTouch(0).position;
        return true;
    }

    /// <summary>
    /// Desktop commits on click, mobile commits when the finger lifts so the
    /// player can drag around and preview before choosing.
    /// </summary>
    public static bool SelectCommitted()
    {
        if (!IsMobile) return Input.GetMouseButtonDown(0);

        return Input.touchCount > 0
               && Input.GetTouch(0).phase is TouchPhase.Ended or TouchPhase.Canceled;
    }
}
