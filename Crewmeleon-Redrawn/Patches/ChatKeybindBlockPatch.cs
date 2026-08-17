using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.Roles;
using HarmonyLib;
using MiraAPI.GameModes;
using MiraAPI.Keybinds;

namespace CrewmeleonRedrawn.Patches;

[HarmonyPatch]
public class ChatKeybindBlockPatch
{
    [HarmonyPatch(typeof(BaseKeybind), nameof(BaseKeybind.Invoke))]
    [HarmonyPrefix]
    public static bool BaseKeybind_Invoke_Prefix(BaseKeybind __instance)
    {
        if (!CustomGameModeManager.IsActiveGameMode<ChameleonGameMode>()) return true;
        if (HudManager.Instance == null) return true;
        if (HudManager.Instance.Chat == null) return true;
        if (HudManager.Instance.Chat.IsOpenOrOpening) return false;
        if (ChameleonGameModeManager.Instance == null) return true;
        if (ChameleonGameModeManager.Instance.Timer.CurrentStage is TimerStage.Hiding && PlayerControl.LocalPlayer.Data.Role is SeekerRole) return false;
        return true;
    }
}