using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The guard's brain. It holds the CURRENT STATE and lets it decide what to do.
/// It also gives the states the basic actions: move to a point (going around walls with A*) and turn.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(GuardVision))]
public class GuardAI : MonoBehaviour
{
    private const float RepathInterval = 0.3f;

    [Header("Patrol route")]
    [Tooltip("Points the guard walks through, in order.")]
    [SerializeField] private Transform[] patrolPoints;

    [Tooltip("Seconds the guard waits at each patrol point.")]
    [SerializeField] private float waitTime = 1f;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 2f;

    // NEW: faster when something is going on. The thief (speed 5) can still outrun a chasing guard.
    [SerializeField] private float investigateSpeed = 2.5f;
    [SerializeField] private float chaseSpeed = 4f;

    [Tooltip("Degrees per second the guard turns.")]
    [SerializeField] private float turnSpeed = 360f;

    [Tooltip("Distance at which the guard counts as having reached a point.")]
    [SerializeField] private float arriveDistance = 0.05f;

    [Tooltip("Half the size of the guard. Used to check if it fits through a gap.")]
    [SerializeField] private float bodyRadius = 0.45f;

    private readonly List<Vector2> path = new List<Vector2>();
    private readonly List<RaycastHit2D> hits = new List<RaycastHit2D>();
    private ContactFilter2D solidOnly;
    private Rigidbody2D rb;
    private NavGrid grid;
    private Vector2 pathDestination;
    private float repathTimer;

    public Transform[] PatrolPoints => patrolPoints;
    public float WaitTime => waitTime;
    public float PatrolSpeed => patrolSpeed;
    public float InvestigateSpeed => investigateSpeed; // NEW
    public float ChaseSpeed => chaseSpeed;             // NEW
    public GuardVision Vision { get; private set; }
    public IGuardState CurrentState { get; private set; }

    // Announced when a guard touches the thief (Observer pattern).
    public static event System.Action ThiefCaught;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Vision = GetComponent<GuardVision>();
        solidOnly = new ContactFilter2D { useTriggers = false };

        grid = FindAnyObjectByType<NavGrid>();
        if (grid == null)
        {
            Debug.LogWarning("GuardAI: there is no NavGrid in the scene, guards will walk in straight lines.");
        }
    }

    private void Start()
    {
        ChangeState(new PatrolState(this));
    }

    private void FixedUpdate()
    {
        CurrentState?.Tick();
    }

    // NEW: Unity calls this when the guard bumps into something. If it's the thief: caught!
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerMovement>() != null)
        {
            ThiefCaught?.Invoke();
        }
    }


    /// <summary>Leaves the current state and enters the new one.</summary>
    public void ChangeState(IGuardState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        Debug.Log($"{name} -> {newState.GetType().Name}");
        CurrentState.Enter();
    }

    /// <summary>
    /// Walks towards a destination, going through doors if there are walls in the way.
    /// Returns true when it has arrived (or got as close as possible).
    /// </summary>
    public bool MoveTo(Vector2 destination, float speed)
    {
        // 1. Nothing in the way (or no grid): walk straight there.
        if (grid == null || HasClearPath(rb.position, destination))
        {
            path.Clear();
            return MoveTowards(destination, speed);
        }

        // 2. Walls in the way: ask A* for a path (again if the destination has moved).
        repathTimer -= Time.fixedDeltaTime;
        bool destinationMoved = Vector2.Distance(destination, pathDestination) > grid.CellSize;
        if (path.Count == 0 || (destinationMoved && repathTimer <= 0f))
        {
            path.Clear();
            path.AddRange(AStar.FindPath(grid, rb.position, destination));
            pathDestination = destination;
            repathTimer = RepathInterval;
            if (path.Count == 0)
            {
                return true; // There is no way to get there: give up.
            }
        }

        // 3. Skip path points we can already reach in a straight line (smoother walking).
        while (path.Count > 1 && HasClearPath(rb.position, path[1]))
        {
            path.RemoveAt(0);
        }

        // 4. Walk to the next point of the path.
        if (MoveTowards(path[0], speed))
        {
            path.RemoveAt(0);
            if (path.Count == 0)
            {
                return true; // End of the path: as close as we can get.
            }
        }

        return false;
    }

    /// <summary>Takes one small step in a straight line. Returns true when it has arrived.</summary>
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

    /// <summary>NEW: spins on the spot, as if looking around.</summary>
    public void LookAround(float degreesPerSecond)
    {
        rb.MoveRotation(rb.rotation + degreesPerSecond * Time.fixedDeltaTime);
    }


    /// <summary>NEW: a random spot near a point where the guard can stand (not inside a wall).</summary>
    public Vector2 GetRandomPointNear(Vector2 center, float radius)
    {
        for (int attempt = 0; attempt < 15; attempt++)
        {
            Vector2 candidate = center + Random.insideUnitCircle * radius;
            bool walkable = grid == null || grid.IsWalkable(grid.WorldToCell(candidate));
            if (walkable)
            {
                return candidate;
            }
        }

        return center;
    }

    /// <summary>Can the guard's whole body slide from one point to another without touching a wall?</summary>
    public bool HasClearPath(Vector2 from, Vector2 to)
    {
        Vector2 offset = to - from;
        float distance = offset.magnitude;
        if (distance < 0.001f)
        {
            return true;
        }

        int count = Physics2D.CircleCast(from, bodyRadius, offset / distance, solidOnly, hits, distance);
        for (int i = 0; i < count; i++)
        {
            if (GuardVision.IsWall(hits[i].collider))
            {
                return false;
            }
        }

        return true;
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

    private void OnDrawGizmos()
    {
        // Patrol route in yellow.
        if (patrolPoints != null)
        {
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

        // Current A* path in green.
        Gizmos.color = Color.green;
        Vector3 previous = transform.position;
        foreach (Vector2 point in path)
        {
            Gizmos.DrawLine(previous, point);
            previous = point;
        }
    }
}