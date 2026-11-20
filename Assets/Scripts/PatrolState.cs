using UnityEngine;

/// <summary>PATROL: walk from point to point, waiting a moment at each one.</summary>
public class PatrolState : IGuardState
{
    private readonly GuardAI guard;
    private int currentPoint;
    private float waitTimer;

    public PatrolState(GuardAI guard)
    {
        this.guard = guard;
    }

    public void Enter()
    {
        // Start from the closest point (useful when coming back from a chase).
        currentPoint = guard.GetClosestPatrolPoint();
        waitTimer = 0f;
    }

    public void Tick()
    {
        Transform[] points = guard.PatrolPoints;
        if (points == null || points.Length == 0)
        {
            return;
        }

        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Transform point = points[currentPoint];
        if (point == null)
        {
            GoToNextPoint();
            return;
        }

        bool arrived = guard.MoveTo(point.position, guard.PatrolSpeed);
        if (arrived)
        {
            GoToNextPoint();
            waitTimer = guard.WaitTime;
        }
    }

    public void Exit()
    {
    }

    private void GoToNextPoint()
    {
        currentPoint = (currentPoint + 1) % guard.PatrolPoints.Length;
    }
}