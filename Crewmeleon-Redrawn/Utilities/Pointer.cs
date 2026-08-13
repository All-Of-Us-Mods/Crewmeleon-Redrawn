using MiraAPI;
using UnityEngine;

namespace CrewmeleonRedrawn.Utilities;

/// <summary>mouse on desktop, first touch on mobile</summary>
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

    /// <summary>desktop commits on click, mobile waits for the finger to lift so you can preview</summary>
    public static bool SelectCommitted()
    {
        if (!IsMobile) return Input.GetMouseButtonDown(0);

        return Input.touchCount > 0
               && Input.GetTouch(0).phase is TouchPhase.Ended or TouchPhase.Canceled;
    }
}
