
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeistSessionManager : MonoBehaviour
{
    public enum HeistOutcome
    {
        Playing,
        Caught,
        Escaped
    }

    public class RoomStats
    {
        public int Attempts { get; private set; }
        public int Entries { get; private set; }
        public float TotalTime { get; private set; }

        private int lastAttemptVisited = -1;

        public void RegisterEntry(int attempt)
        {
            Entries++;

            if (lastAttemptVisited != attempt)
            {
                Attempts++;
                lastAttemptVisited = attempt;
            }
        }

        public void AddTime(float seconds)
        {
            TotalTime += seconds;
        }
    }

    public static HeistSessionManager Instance { get; private set; }

    [SerializeField] private int maxAttempts = 5;

    public int CurrentAttempt { get; private set; } = 1;
    public int MaxAttempts => maxAttempts;
    public bool IsLastAttempt => CurrentAttempt >= maxAttempts;

    public HeistOutcome Outcome { get; private set; }
        = HeistOutcome.Playing;

    private Dictionary<string, RoomStats> roomMemory =
        new Dictionary<string, RoomStats>();

    public IReadOnlyDictionary<string, RoomStats> RoomMemory
        => roomMemory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void MarkCaught()
    {
        if (Outcome != HeistOutcome.Playing)
            return;

        Outcome = HeistOutcome.Caught;
    }

    public void MarkEscaped()
    {
        if (Outcome != HeistOutcome.Playing)
            return;

        Outcome = HeistOutcome.Escaped;
    }

    public void ContinueAfterResult()
    {
        if (Outcome == HeistOutcome.Caught && !IsLastAttempt)
        {
            CurrentAttempt++;
            Outcome = HeistOutcome.Playing;
            ReloadMuseum();
        }
        else if (Outcome == HeistOutcome.Escaped ||
                 (Outcome == HeistOutcome.Caught && IsLastAttempt))
        {
            StartNewHeist();
        }
    }

    public void StartNewHeist()
    {
        CurrentAttempt = 1;
        Outcome = HeistOutcome.Playing;

        roomMemory.Clear();

        ReloadMuseum();
    }

    private void ReloadMuseum()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    // Room memory

    public void RegisterRoomEntry(string roomId)
    {
        if (Outcome != HeistOutcome.Playing ||
            string.IsNullOrWhiteSpace(roomId))
            return;

        if (!roomMemory.ContainsKey(roomId))
        {
            roomMemory[roomId] = new RoomStats();
        }

        roomMemory[roomId].RegisterEntry(CurrentAttempt);

        RoomStats stats = roomMemory[roomId];

        Debug.Log(
            $"[MEMORY] {roomId} | " +
            $"Attempts: {stats.Attempts} | " +
            $"Entries: {stats.Entries}"
        );
    }

    public void AddRoomTime(string roomId, float seconds)
    {
        if (Outcome != HeistOutcome.Playing)
            return;

        if (roomMemory.TryGetValue(roomId, out RoomStats stats))
        {
            stats.AddTime(seconds);
        }
    }

    public RoomStats GetRoomStats(string roomId)
    {
        roomMemory.TryGetValue(roomId, out RoomStats stats);
        return stats;
    }
}

