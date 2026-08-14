namespace CrewmeleonRedrawn.States;

public static class StateManager
{
    private static readonly Dictionary<PlayerControl, Dictionary<PlayerModifier, List<StateModifier>>> PlayerModifiers = new();
    
    public static StateModifier AddPlayerModifier(PlayerControl player, PlayerModifier modifier, Func<bool> condition,
        string reason)
    {
        if (!PlayerModifiers.TryGetValue(player, out var modifiers))
            modifiers = PlayerModifiers[player] = new Dictionary<PlayerModifier, List<StateModifier>>();
        
        if (!modifiers.TryGetValue(modifier, out var conditions))
            conditions = modifiers[modifier] = [];
        
        var state = new StateModifier(condition, reason);
        conditions.Add(state);
        return state;
    }
    
    public static bool EvaluatePlayerModifiers(PlayerControl player, PlayerModifier modifier, bool defaultValue = true)
    {
        if (!player
            || !PlayerModifiers.TryGetValue(player, out var modifiers)
            || !modifiers.TryGetValue(modifier, out var conditions))
            return defaultValue;
        
        conditions.RemoveAll(state => state.IsDisposed);
        
        return conditions.Count == 0 ? defaultValue : conditions.All(state => state.Condition());
    }
    
    public static void ClearAll() => PlayerModifiers.Clear();
}