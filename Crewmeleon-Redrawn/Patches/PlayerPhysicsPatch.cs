using CrewmeleonRedrawn.States;
using HarmonyLib;

namespace CrewmeleonRedrawn.Patches;

/*
 * state doesnt care about buttons, shouldnt break anything, if it does we can get another modifier for buttons
 * or add them to the modifier
 */
[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.TrueSpeed), MethodType.Getter)]
public static class PlayerPhysicsPatch
{
    public static void Postfix(PlayerPhysics __instance, ref float __result)
    {
        if (!__instance.myPlayer.CanMove())
            __result = 0;
    }
}