using MiraAPI.Hud;
using MiraAPI.Patches;
using MiraAPI.Utilities;

namespace Crewmeleon_Redrawn.Utilities;

public static class CustomButtonUtilities
{
    private static readonly Dictionary<CustomActionButton, DateTimeOffset> LastVisible = new();

    /// <summary>
    /// Re-evaluates every custom button's <c>Enabled(role)</c> immediately. Without this the row
    /// keeps its old layout until the grid next rearranges itself, which reads as a dropped frame.
    /// </summary>
    public static void RefreshVisibility()
    {
        var role = PlayerControl.LocalPlayer?.Data?.Role;
        if (role == null) return;

        foreach (var button in CustomButtonManager.Buttons)
        {
            var enabled = button.Enabled(role);
            button.SetActive(enabled, role);

            if (!enabled)
            {
                LastVisible.TryAdd(button, DateTimeOffset.UtcNow);
                continue;
            }

            // charge the cooldown for the time spent hidden, so toggling a mode can't pause or refund it
            if (!LastVisible.Remove(button, out var lastVisible)) continue;

            var hidden = (float) (DateTimeOffset.UtcNow - lastVisible).TotalSeconds;
            button.Timer = MathF.Max(0f, button.Timer - hidden);
            button.Button.SetCooldownFormat(button.Timer, button.Cooldown, button.CooldownTimerFormatString);
        }

        var bottomLeft = HudManagerPatches.BottomLeft;
        if (bottomLeft != null) bottomLeft.GetComponent<GridArrange>().CheckCurrentChildren();
    }
}
