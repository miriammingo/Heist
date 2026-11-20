/// <summary>
/// STATE pattern: each thing a guard can be doing (patrolling, investigating,
/// chasing, searching) is its own class that implements this interface.
/// </summary>
public interface IGuardState
{
    /// <summary>Called once when the guard switches to this state.</summary>
    void Enter();

    /// <summary>Called every physics step (FixedUpdate) while this state is active.</summary>
    void Tick();

    /// <summary>Called once when the guard leaves this state.</summary>
    void Exit();
}
