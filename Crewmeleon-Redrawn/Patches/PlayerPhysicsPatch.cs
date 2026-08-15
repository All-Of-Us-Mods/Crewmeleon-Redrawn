using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.States;
using HarmonyLib;

namespace CrewmeleonRedrawn.Patches;

/*
 * state doesnt care about buttons, shouldnt break anything, if it does we can get another modifier for buttons
 * or add them to the modifier
 */
[HarmonyPatch(typeof(PlayerPhysics))]
public static class PlayerPhysicsPatch
{
    [HarmonyPatch(nameof(PlayerPhysics.TrueSpeed), MethodType.Getter)]     [HarmonyPrefix]
    public static bool TrueSpeedPatch(PlayerPhysics __instance, ref float __result)
    {
        if (!__instance.myPlayer.CanMove())
        {
            __result = 0f;
            return false;
        }
        if (!ChameleonGameModeManager.Instance) return true;
        __result = __instance.Speed * ChameleonGameModeManager.Instance!.GetPlayerSpeed(__instance.myPlayer);
        return false;
    }
}