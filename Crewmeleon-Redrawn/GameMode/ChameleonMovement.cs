using CrewmeleonRedrawn.Buttons.Hider;
using CrewmeleonRedrawn.Modifiers;
using CrewmeleonRedrawn.States;
using MiraAPI.Hud;
using MiraAPI.Modifiers;

namespace CrewmeleonRedrawn.GameMode;

public static class ChameleonMovement
{
    // this is cleaned up on scene transition, will always be on the player from the start of the game
    public static void RegisterBlocks(PlayerControl player) =>
        player.BlockMovementWhile(() => IsBlocked(player), "chameleon movement rules");
    
    private static bool IsBlocked(PlayerControl player)
    {
        if (!player.Data || player.Data.IsDead)
            return false;
        
        if (player.HasModifier<PaintingModifier>() || player.HasModifier<SpectatingModifier>()) // block painters / spectators from moving
            return true;
        
        var stage = ChameleonGameModeManager.Instance?.CurrentStage;
        
        if (stage == TimerStage.Revelation) // block everyone from moving in position reveal
            return true;
        
        if (ChameleonGameMode.AmImpostor) // block hunters from moving in cutscene
            return stage == TimerStage.Hiding;
        
        // locked button, only run if they can actually toggle locked state (button is visible)
        var lockButton = CustomButtonSingleton<LockMovementButton>.Instance;
        
        return lockButton is { IsLocked: true } && lockButton.Enabled(player.Data.Role);
    }
}
