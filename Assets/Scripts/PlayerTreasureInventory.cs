using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTreasureInventory : MonoBehaviour
{
    private List<string> collectedTreasures = new List<string>();

    public int TreasureCount => collectedTreasures.Count;

    public bool HasTreasure => TreasureCount > 0;

    // NEW: announced every time the thief picks up a treasure (Observer pattern).
    // It sends the new total, so whoever listens knows how many treasures the thief has.
    public event Action<int> TreasureCollected;

    public void CollectTreasure(string treasureName)
    {
        collectedTreasures.Add(treasureName);

        Debug.Log("Got the " + treasureName + ". Like I was never here.");

        // NEW: tell everyone who is listening.
        TreasureCollected?.Invoke(TreasureCount);
    }
}


