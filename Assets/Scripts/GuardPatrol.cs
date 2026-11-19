using UnityEngine;

/// <summary>
/// Moves the guard between patrol points in a loop: A -> B -> A -> B...
/// Attach it to the Guard object, which needs a Rigidbody2D (Body Type = Kinematic).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class GuardPatrol : MonoBehaviour
{
    [Header("Patrol route")]
    [Tooltip("Points the guard walks through, in order. Drag PatrolPointA and PatrolPointB here.")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Movement")]
    [Tooltip("Guard speed (units per second).")]
    [SerializeField] private float speed = 2f;

    [Tooltip("Seconds the guard waits at each point before moving on.")]
    [SerializeField] private float waitTime = 1f;

    [Tooltip("Distance at which the guard counts as having reached a point.")]
    [SerializeField] private float arriveDistance = 0.05f;

    // NEW: how fast the guard turns to look where it is walking.
    [Tooltip("How fast the guard turns to face where it walks (degrees per second).")]
    [SerializeField] private float turnSpeed = 360f;

    private Rigidbody2D rb;
    private int currentPoint;
    private float waitTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Physics movement goes in FixedUpdate (not Update).
    private void FixedUpdate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        // 1. If the guard is waiting at a point, count down and stay still.
        if (waitTimer > 0f)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        // 2. Skip empty slots in the list.
        Transform point = patrolPoints[currentPoint];
        if (point == null)
        {
            GoToNextPoint();
            return;
        }

        // 3. Take a small step towards the current point...
        Vector2 target = point.position;
        Vector2 newPosition = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        // NEW: ...and turn to look in the direction it is walking.
        FaceDirection(target - rb.position);

        // 4. When the point is reached, wait a moment and target the next one.
        if (Vector2.Distance(newPosition, target) <= arriveDistance)
        {
            GoToNextPoint();
            waitTimer = waitTime;
        }
    }

    private void GoToNextPoint()
    {
        // The % wraps around to the first point after the last one (loop).
        currentPoint = (currentPoint + 1) % patrolPoints.Length;
    }

    // NEW: rotates the guard so its "up" side points in the given direction.
    // Later, the vision cones will point where the guard's "up" points.
    private void FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        // Atan2 gives the angle of the direction. We subtract 90 because the guard looks "up", not "right".
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);
    }

    // Draws the route in yellow in the Scene view (not visible in the game).
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
