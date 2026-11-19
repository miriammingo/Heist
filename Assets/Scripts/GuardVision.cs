using System.Collections.Generic;
using UnityEngine;

/// <summary>What the guard knows about the thief right now.</summary>
public enum Awareness
{
    None,        // Can't see the thief.
    Suspicious,  // Sees the thief far away (outer cone).
    Spotted      // Sees the thief close (inner cone).
}

/// <summary>
/// The guard's eyes: two vision cones pointing where the guard looks (its "up" side).
///  - Outer cone (long and narrow): the guard gets suspicious.
///  - Inner cone (short and wide): the guard has spotted the thief.
/// Walls block the view.
/// </summary>
public class GuardVision : MonoBehaviour
{
    [Header("Inner cone (spotted)")]
    [SerializeField] private float nearRange = 3.5f;
    [SerializeField, Range(10f, 180f)] private float nearAngle = 80f;

    [Header("Outer cone (suspicious)")]
    [SerializeField] private float farRange = 7f;
    [SerializeField, Range(10f, 180f)] private float farAngle = 50f;

    private readonly List<RaycastHit2D> hits = new List<RaycastHit2D>();
    private ContactFilter2D solidOnly;
    private Transform thief;

    public float NearRange => nearRange;
    public float NearAngle => nearAngle;
    public float FarRange => farRange;
    public float FarAngle => farAngle;
    public Vector2 Facing => transform.up;
    public Awareness CurrentAwareness { get; private set; }

    private void Awake()
    {
        // Rays ignore triggers (treasures, exit), so those never block the view.
        solidOnly = new ContactFilter2D { useTriggers = false };

        // Find the thief automatically: it is the object with the PlayerMovement script.
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player != null)
        {
            thief = player.transform;
        }
        else
        {
            Debug.LogWarning("GuardVision: there is no Player in the scene.");
        }
    }

    private void Update()
    {
        Awareness newAwareness = thief == null ? Awareness.None : Look(thief.position);
        if (newAwareness != CurrentAwareness)
        {
            Debug.Log($"{name}: {CurrentAwareness} -> {newAwareness}");
            CurrentAwareness = newAwareness;
        }
    }

    /// <summary>Checks both cones. The inner cone wins because it is more dangerous.</summary>
    public Awareness Look(Vector2 target)
    {
        if (IsInsideCone(target, nearRange, nearAngle) && HasLineOfSight(target))
        {
            return Awareness.Spotted;
        }

        if (IsInsideCone(target, farRange, farAngle) && HasLineOfSight(target))
        {
            return Awareness.Suspicious;
        }

        return Awareness.None;
    }

    /// <summary>Is the target close enough AND inside the cone's angle?</summary>
    public bool IsInsideCone(Vector2 target, float range, float angle)
    {
        Vector2 toTarget = target - (Vector2)transform.position;
        bool closeEnough = toTarget.sqrMagnitude <= range * range;
        bool insideAngle = Vector2.Angle(Facing, toTarget) <= angle * 0.5f;
        return closeEnough && insideAngle;
    }

    /// <summary>Is there no wall between the guard and the target?</summary>
    public bool HasLineOfSight(Vector2 target)
    {
        Vector2 toTarget = target - (Vector2)transform.position;
        float distance = toTarget.magnitude;
        if (distance < 0.001f)
        {
            return true;
        }

        return DistanceToWall(toTarget / distance, distance) >= distance;
    }

    /// <summary>How far a ray travels before hitting a wall (maxDistance if it hits nothing).</summary>
    public float DistanceToWall(Vector2 direction, float maxDistance)
    {
        int count = Physics2D.Raycast(transform.position, direction, solidOnly, hits, maxDistance);
        float closest = maxDistance;
        for (int i = 0; i < count; i++)
        {
            if (IsWall(hits[i].collider) && hits[i].distance < closest)
            {
                closest = hits[i].distance;
            }
        }

        return closest;
    }

    // Walls are solid colliders WITHOUT a Rigidbody2D.
    // The thief and the guards have a Rigidbody2D, so they don't block the view.
    private static bool IsWall(Collider2D other)
    {
        return !other.isTrigger && other.attachedRigidbody == null;
    }

    // Draws both cones in the Scene view: white = calm, yellow = suspicious, red = spotted.
    private void OnDrawGizmos()
    {
        Gizmos.color = CurrentAwareness == Awareness.Spotted ? Color.red
                     : CurrentAwareness == Awareness.Suspicious ? Color.yellow
                     : Color.white;
        DrawConeGizmo(farRange, farAngle);
        DrawConeGizmo(nearRange, nearAngle);
    }

    private void DrawConeGizmo(float range, float angle)
    {
        const int segments = 16;
        Vector3 origin = transform.position;
        Vector3 previous = origin;
        for (int i = 0; i <= segments; i++)
        {
            float a = Mathf.Lerp(-angle * 0.5f, angle * 0.5f, i / (float)segments);
            Vector3 point = origin + Quaternion.Euler(0f, 0f, a) * transform.up * range;
            Gizmos.DrawLine(previous, point);
            previous = point;
        }

        Gizmos.DrawLine(previous, origin);
    }
}