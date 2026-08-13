using MiraAPI.Hud;
using MiraAPI.Patches;
using MiraAPI.Utilities;
using Object = UnityEngine.Object;

namespace CrewmeleonRedrawn.Utilities;

public static class CustomButtonUtilities
{
    private static readonly Dictionary<CustomActionButton, DateTimeOffset> LastVisible = new();

    /// <summary>
    /// re checks every buttons Enabled(role) right now. without it the row keeps its old layout
    /// until the grid next rearranges, which looks like a dropped frame
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

            // charge the time it spent hidden so toggling cant pause or refund a cooldown
            if (!LastVisible.Remove(button, out var lastVisible)) continue;

            var hidden = (float) (DateTimeOffset.UtcNow - lastVisible).TotalSeconds;
            button.Timer = MathF.Max(0f, button.Timer - hidden);
            button.Button.SetCooldownFormat(button.Timer, button.Cooldown, button.CooldownTimerFormatString);
        }

        var bottomLeft = HudManagerPatches.BottomLeft;
        if (bottomLeft != null) bottomLeft.GetComponent<GridArrange>().CheckCurrentChildren();
    }

    public static bool IsInPractice()
    {
        return Object.FindObjectOfType<TutorialManager>() != null;
    }
}
