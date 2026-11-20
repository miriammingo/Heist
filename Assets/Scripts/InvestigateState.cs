using UnityEngine;

/// <summary>
/// INVESTIGATE: the guard saw something far away (outer cone).
/// It walks to that spot and looks around. If it sees the thief close up, it starts chasing.
/// If it finds nothing, it goes back to patrolling.
/// </summary>
public class InvestigateState : IGuardState
{
    private const float LookAroundTime = 2f;
    private const float LookAroundSpeed = 120f;

    private readonly GuardAI guard;
    private Vector2 target;
    private bool arrived;
    private float lookTimer;

    public InvestigateState(GuardAI guard, Vector2 target)
    {
        this.guard = guard;
        this.target = target;
    }

    public void Enter()
    {
        arrived = false;
        lookTimer = 0f;
    }

    public void Tick()
    {
        switch (guard.Vision.CurrentAwareness)
        {
            case Awareness.Spotted:
                guard.ChangeState(new ChaseState(guard));
                return;
            case Awareness.Suspicious:
                // Still sees something: keep walking towards it.
                target = guard.Vision.ThiefPosition;
                arrived = false;
                break;
        }

        if (!arrived)
        {
            arrived = guard.MoveTo(target, guard.InvestigateSpeed);
            return;
        }

        // At the spot: look around for a moment, then go back to work.
        guard.LookAround(LookAroundSpeed);
        lookTimer += Time.fixedDeltaTime;
        if (lookTimer >= LookAroundTime)
        {
            guard.ChangeState(new PatrolState(guard));
        }
    }

    public void Exit()
    {
    }
}
