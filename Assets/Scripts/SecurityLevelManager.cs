using UnityEngine;

/// <summary>
/// PROGRESSIVE SECURITY: the more treasures the thief steals, the more guards there are.
///   1 treasure  -> the 1st reinforcement guard appears.
///   2 treasures -> the 2nd reinforcement guard appears.
///   3 treasures -> HIGH ALERT (no more guards: 4 is the maximum).
/// It LISTENS to PlayerTreasureInventory.TreasureCollected (Observer pattern).
/// The reinforcements are normal Guard prefabs placed in the scene and disabled,
/// so they use exactly the same AI as the other guards.
/// Put it on an empty object called "SecurityLevelManager" in the Museum scene.
/// </summary>
public class SecurityLevelManager : MonoBehaviour
{
    [Tooltip("Disabled guards, in the order they appear (1st treasure, 2nd treasure...).")]
    [SerializeField] private GuardAI[] reinforcements;

    [Tooltip("Number of treasures that turns on HIGH ALERT.")]
    [SerializeField] private int highAlertAt = 3;

    private PlayerTreasureInventory thief;

    public bool IsHighAlert { get; private set; }

    private void Awake()
    {
        thief = FindAnyObjectByType<PlayerTreasureInventory>();
        if (thief == null)
        {
            Debug.LogWarning("SecurityLevelManager: there is no Player in the scene.");
        }
    }

    // Start listening when this object is active, stop when it isn't.
    private void OnEnable()
    {
        if (thief != null)
        {
            thief.TreasureCollected += HandleTreasureCollected;
        }
    }

    private void OnDisable()
    {
        if (thief != null)
        {
            thief.TreasureCollected -= HandleTreasureCollected;
        }
    }

    private void HandleTreasureCollected(int treasureCount)
    {
        // 1st treasure -> reinforcements[0], 2nd treasure -> reinforcements[1]...
        int index = treasureCount - 1;
        if (index >= 0 && index < reinforcements.Length)
        {
            Deploy(reinforcements[index]);
        }

        if (treasureCount >= highAlertAt && !IsHighAlert)
        {
            StartHighAlert();
        }
    }

    /// <summary>Turns on a reinforcement guard, far away from the thief.</summary>
    private void Deploy(GuardAI guard)
    {
        if (guard == null || guard.gameObject.activeSelf)
        {
            return;
        }

        // It appears at its patrol point that is FARTHEST from the thief,
        // so it never pops up right on top of them.
        Transform spawnPoint = GetFarthestPatrolPoint(guard);
        if (spawnPoint != null)
        {
            guard.transform.position = spawnPoint.position;
        }

        guard.gameObject.SetActive(true);
        Debug.Log($"[SECURITY] {guard.name} deployed!");
    }

    private Transform GetFarthestPatrolPoint(GuardAI guard)
    {
        Transform farthest = null;
        float farthestDistance = -1f;
        foreach (Transform point in guard.PatrolPoints)
        {
            if (point == null)
            {
                continue;
            }

            float distance = Vector2.Distance(point.position, thief.transform.position);
            if (distance > farthestDistance)
            {
                farthestDistance = distance;
                farthest = point;
            }
        }

        return farthest;
    }

    private void StartHighAlert()
    {
        IsHighAlert = true;
        Debug.Log("[SECURITY] HIGH ALERT!");
    }
}
