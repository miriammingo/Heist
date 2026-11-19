using System.Collections.Generic;
using UnityEngine;

public class PlayerTreasureInventory : MonoBehaviour
{
    private List<string> collectedTreasures = new List<string>();

    public int TreasureCount => collectedTreasures.Count;

    public bool HasTreasure => TreasureCount > 0;

    public void CollectTreasure(string treasureName)
    {
        collectedTreasures.Add(treasureName);

        Debug.Log("Got the " + treasureName + ". Like I was never here.");
    }
}


