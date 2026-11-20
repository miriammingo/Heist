
using UnityEngine;

public class MuseumRoomZone : MonoBehaviour
{
    [SerializeField] private string roomId = "Gallery_A";

    private bool playerInside = false;
    private bool visitedThisAttempt = false;
    private float timeInside = 0f;

    // Persistent keys for each room
    private string VisitsKey => "Heist_Visits_" + roomId;
    private string EntriesKey => "Heist_Entries_" + roomId;
    private string TimeKey => "Heist_Time_" + roomId;

    private void Start()
    {
        Debug.Log(
            $"[ROOM MEMORY] {roomId} | " +
            $"Attempts: {PlayerPrefs.GetInt(VisitsKey, 0)} | " +
            $"Entries: {PlayerPrefs.GetInt(EntriesKey, 0)} | " +
            $"Total time: {PlayerPrefs.GetFloat(TimeKey, 0f):F1}s"
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerTreasureInventory player =
            other.GetComponentInParent<PlayerTreasureInventory>();

        if (player == null || playerInside)
            return;

        playerInside = true;

        // Count EVERY entry into the room
        int entries = PlayerPrefs.GetInt(EntriesKey, 0) + 1;
        PlayerPrefs.SetInt(EntriesKey, entries);

        // Count only ONCE per attempt
        if (!visitedThisAttempt)
        {
            visitedThisAttempt = true;

            int visits = PlayerPrefs.GetInt(VisitsKey, 0) + 1;
            PlayerPrefs.SetInt(VisitsKey, visits);
        }

        PlayerPrefs.Save();

        Debug.Log(
            $"[ROOM MEMORY] Entered {roomId} | " +
            $"Attempts: {PlayerPrefs.GetInt(VisitsKey)} | " +
            $"Entries: {entries}"
        );
    }

    private void Update()
    {
        if (playerInside)
        {
            timeInside += Time.deltaTime;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerTreasureInventory player =
            other.GetComponentInParent<PlayerTreasureInventory>();

        if (player == null || !playerInside)
            return;

        SaveTime();
        playerInside = false;
    }

    private void OnDisable()
    {
        // Save pending time when the scene closes
        SaveTime();
        playerInside = false;
    }

    private void SaveTime()
    {
        if (timeInside <= 0f)
            return;

        float totalTime =
            PlayerPrefs.GetFloat(TimeKey, 0f) + timeInside;

        PlayerPrefs.SetFloat(TimeKey, totalTime);
        PlayerPrefs.Save();

        Debug.Log(
            $"[ROOM MEMORY] {roomId} | " +
            $"Total time: {totalTime:F1}s"
        );

        timeInside = 0f;
    }
}

