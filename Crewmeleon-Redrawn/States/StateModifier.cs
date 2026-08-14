namespace CrewmeleonRedrawn.States;

public class StateModifier(Func<bool> condition, string reason) : IDisposable
{
    public Func<bool> Condition { get; } = condition;
    public string Reason { get; } = reason;
    public bool IsDisposed { get; private set; }
    
    public void Dispose() => IsDisposed = true;
}