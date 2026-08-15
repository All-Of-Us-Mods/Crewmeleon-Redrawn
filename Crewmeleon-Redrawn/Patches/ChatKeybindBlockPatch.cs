using HarmonyLib;
using MiraAPI.Keybinds;

namespace CrewmeleonRedrawn.Patches;

[HarmonyPatch]
public class ChatKeybindBlockPatch
{
    [HarmonyPatch(typeof(BaseKeybind), nameof(BaseKeybind.Invoke))]
    [HarmonyPrefix]
    public static bool BaseKeybind_Invoke_Prefix(BaseKeybind __instance)
    {
        if (HudManager.Instance == null) return true;
        if (HudManager.Instance.Chat == null) return true;
        if (HudManager.Instance.Chat.IsOpenOrOpening) return false;
        return true;
    }
}