using UnityEngine;

/// <summary>
/// SEARCH: the guard has lost the thief. It checks a few spots around the last place
/// it saw them, looking around at each one. It is on HIGH ALERT: if it sees the thief
/// in either cone, it chases again. When it has checked every spot, it goes back to patrolling.
/// </summary>
public class SearchState : IGuardState
{
    private const int SpotsToCheck = 3;
    private const float SearchRadius = 3f;
    private const float LookAroundTime = 1.5f;
    private const float LookAroundSpeed = 150f;

    private readonly GuardAI guard;
    private readonly Vector2 searchCenter;
    private Vector2 currentSpot;
    private int spotsChecked;
    private bool arrived;
    private float lookTimer;

    public SearchState(GuardAI guard, Vector2 searchCenter)
    {
        this.guard = guard;
        this.searchCenter = searchCenter;
    }

    public void Enter()
    {
        // The first spot is exactly where the thief was last seen.
        currentSpot = searchCenter;
        spotsChecked = 0;
        arrived = false;
        lookTimer = 0f;
    }

    public void Tick()
    {
        // High alert: seeing the thief in ANY cone means chasing again.
        if (guard.Vision.CurrentAwareness != Awareness.None)
        {
            guard.ChangeState(new ChaseState(guard));
            return;
        }

        if (!arrived)
        {
            arrived = guard.MoveTo(currentSpot, guard.InvestigateSpeed);
            return;
        }

        // At a spot: look around for a moment.
        guard.LookAround(LookAroundSpeed);
        lookTimer += Time.fixedDeltaTime;
        if (lookTimer < LookAroundTime)
        {
            return;
        }

        spotsChecked++;
        if (spotsChecked >= SpotsToCheck)
        {
            guard.ChangeState(new PatrolState(guard));
            return;
        }

        // Next spot: somewhere random near the last place the thief was seen.
        currentSpot = guard.GetRandomPointNear(searchCenter, SearchRadius);
        arrived = false;
        lookTimer = 0f;
    }

    public void Exit()
    {
    }
}