using CrewmeleonRedrawn.GameMode;
using CrewmeleonRedrawn.States;
using HarmonyLib;

namespace CrewmeleonRedrawn.Patches;

/*
 * state doesnt care about buttons, shouldnt break anything, if it does we can get another modifier for buttons
 * or add them to the modifier
 *
 * TrueSpeed is inlined on mobile and SpeedMod is inlined on pc
 */
[HarmonyPatch(typeof(PlayerPhysics))]
public static class PlayerPhysicsPatch
{
    [HarmonyPatch(nameof(PlayerPhysics.SpeedMod), MethodType.Getter)]      [HarmonyPrefix]
    public static bool SpeedModPatch(PlayerPhysics __instance, ref float __result) =>
        !TryGetSpeedMod(__instance, out __result);
    
    [HarmonyPatch(nameof(PlayerPhysics.TrueSpeed), MethodType.Getter)]     [HarmonyPrefix]
    public static bool TrueSpeedPatch(PlayerPhysics __instance, ref float __result)
    {
        if (!TryGetSpeedMod(__instance, out var speedMod)) return true;
        __result = __instance.Speed * speedMod;
        return false;
    }
    
    private static bool TryGetSpeedMod(PlayerPhysics physics, out float speedMod)
    {
        speedMod = 0f;
        var player = physics.myPlayer;
        if (!player.CanMove()) return true;
        if (!ChameleonGameModeManager.Instance) return false;
        
        speedMod = ChameleonGameModeManager.Instance!.GetPlayerSpeed(player);
        if (player.Data && player.Data.IsDead) speedMod *= physics.GhostSpeed / physics.Speed;
        return true;
    }
}