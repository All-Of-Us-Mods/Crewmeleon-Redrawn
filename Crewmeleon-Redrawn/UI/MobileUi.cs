using MiraAPI;
using UnityEngine;

namespace CrewmeleonRedrawn.UI;

/// <summary>drives the mobile layout from a desktop session while working on it</summary>
public static class MobileUi
{
    private const KeyCode ToggleKey = KeyCode.F9;

    private static bool forced;

    public static bool Active => MiraApiPlugin.IsMobile || forced;

    /// <summary>true on the frame the toggle flips, so callers can rebuild what depends on it</summary>
    public static bool PollToggle()
    {
        if (MiraApiPlugin.IsMobile || !Input.GetKeyDown(ToggleKey)) return false;

        forced = !forced;
        return true;
    }
}
