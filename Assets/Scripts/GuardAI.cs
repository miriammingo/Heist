using UnityEngine;

/// <summary>
/// The guard's brain. It holds the CURRENT STATE and lets it decide what to do.
/// It also gives the states the basic actions: move towards a point and turn.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(GuardVision))]
public class GuardAI : MonoBehaviour
{
    [Header("Patrol route")]
    [Tooltip("Points the guard walks through, in order.")]
    [SerializeField] private Transform[] patrolPoints;

    [Tooltip("Seconds the guard waits at each patrol point.")]
    [SerializeField] private float waitTime = 1f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;

    [Tooltip("Degrees per second the guard turns.")]
    [SerializeField] private float turnSpeed = 360f;

    [Tooltip("Distance at which the guard counts as having reached a point.")]
    [SerializeField] private float arriveDistance = 0.05f;

    private Rigidbody2D rb;

    public Transform[] PatrolPoints => patrolPoints;
    public float WaitTime => waitTime;
    public float PatrolSpeed => patrolSpeed;
    public GuardVision Vision { get; private set; }
    public IGuardState CurrentState { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Vision = GetComponent<GuardVision>();
    }

    private void Start()
    {
        ChangeState(new PatrolState(this));
    }

    private void FixedUpdate()
    {
        CurrentState?.Tick();
    }

    /// <summary>Leaves the current state and enters the new one.</summary>
    public void ChangeState(IGuardState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        Debug.Log($"{name} -> {newState.GetType().Name}");
        CurrentState.Enter();
    }

    /// <summary>Takes one small step towards the target. Returns true when it has arrived.</summary>
    public bool MoveTowards(Vector2 target, float speed)
    {
        Vector2 newPosition = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        FaceDirection(target - rb.position);
        rb.MovePosition(newPosition);
        return Vector2.Distance(newPosition, target) <= arriveDistance;
    }

    /// <summary>Turns the guard little by little so its "up" side points in this direction.</summary>
    public void FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);
    }

    /// <summary>Index of the patrol point closest to the guard (to resume the route after a chase).</summary>
    public int GetClosestPatrolPoint()
    {
        int closest = 0;
        float bestDistance = float.MaxValue;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
            {
                continue;
            }

            float distance = Vector2.Distance(rb.position, patrolPoints[i].position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = i;
            }
        }

        return closest;
    }

    // Draws the patrol route in yellow in the Scene view.
    private void OnDrawGizmos()
    {
        if (patrolPoints == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Transform from = patrolPoints[i];
            Transform to = patrolPoints[(i + 1) % patrolPoints.Length];
            if (from == null)
            {
                continue;
            }

            Gizmos.DrawWireSphere(from.position, 0.3f);
            if (to != null)
            {
                Gizmos.DrawLine(from.position, to.position);
            }
        }
    }
}