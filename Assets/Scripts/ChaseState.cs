using UnityEngine;

/// <summary>
/// CHASE: the guard has spotted the thief and runs after them.
/// If the thief gets out of sight, the guard still "knows" where they are for a moment,
/// then runs to the last place it saw them and investigates there.
/// </summary>
public class ChaseState : IGuardState
{
    private const float ForgetTime = 0.6f;

    private readonly GuardAI guard;
    private Vector2 lastKnownPosition;
    private float timeSinceSeen;

    public ChaseState(GuardAI guard)
    {
        this.guard = guard;
    }

    public void Enter()
    {
        lastKnownPosition = guard.Vision.ThiefPosition;
        timeSinceSeen = 0f;
    }

    public void Tick()
    {
        Vector2 thief = guard.Vision.ThiefPosition;
        if (guard.Vision.CanTrack(thief))
        {
            timeSinceSeen = 0f;
        }
        else
        {
            timeSinceSeen += Time.fixedDeltaTime;
        }

        // Can see the thief (or lost them just now): run after them.
        if (timeSinceSeen <= ForgetTime)
        {
            lastKnownPosition = thief;
            guard.MoveTo(lastKnownPosition, guard.ChaseSpeed);
            return;
        }

        // Lost them: run to the last place they were seen, then investigate there.
        bool arrived = guard.MoveTo(lastKnownPosition, guard.ChaseSpeed);
        if (arrived)
        {
            guard.ChangeState(new InvestigateState(guard, lastKnownPosition));
        }
    }

    public void Exit()
    {
    }
}