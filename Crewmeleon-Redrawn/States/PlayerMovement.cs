namespace CrewmeleonRedrawn.States;

public static class PlayerMovement
{
    public static bool CanMove(this PlayerControl player) =>
        StateManager.EvaluatePlayerModifiers(player, PlayerModifier.CanMove);

    public static StateModifier BlockMovement(this PlayerControl player, string reason) =>
        StateManager.AddPlayerModifier(player, PlayerModifier.CanMove, () => false, reason);
    
    public static StateModifier BlockMovementWhile(this PlayerControl player, Func<bool> blocked, string reason) =>
        StateManager.AddPlayerModifier(player, PlayerModifier.CanMove, () => !blocked(), reason);
}