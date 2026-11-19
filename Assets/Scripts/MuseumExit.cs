
using UnityEngine;

public class MuseumExit : MonoBehaviour
{
    [SerializeField] private int requiredTreasures = 3;

    private PlayerTreasureInventory nearbyPlayer;
    private HeistHUD hud;

    private int lastTreasureCount = -1;
    private bool hasWon = false;

    void Start()
    {
        hud = FindAnyObjectByType<HeistHUD>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerTreasureInventory inventory =
            other.GetComponent<PlayerTreasureInventory>();

        if (inventory != null && !hasWon)
        {
            nearbyPlayer = inventory;
            UpdateExitMessage();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerTreasureInventory inventory =
            other.GetComponent<PlayerTreasureInventory>();

        if (inventory != null && inventory == nearbyPlayer)
        {
            nearbyPlayer = null;
            lastTreasureCount = -1;

            if (hud != null && !hasWon)
            {
                hud.HideInteraction();
            }
        }
    }

    void Update()
    {
        if (nearbyPlayer == null || hasWon)
        {
            return;
        }

        // Update the message if the inventory changes.
        if (nearbyPlayer.TreasureCount != lastTreasureCount)
        {
            UpdateExitMessage();
        }

        // Escape only when all required treasures are collected.
        if (nearbyPlayer.TreasureCount >= requiredTreasures &&
            Input.GetKeyDown(KeyCode.E))
        {
            CompleteHeist();
        }
    }

    void UpdateExitMessage()
    {
        lastTreasureCount = nearbyPlayer.TreasureCount;

        if (hud == null)
        {
            return;
        }

        if (nearbyPlayer.TreasureCount >= requiredTreasures)
        {
            hud.ShowInteraction("Press [E] to escape");
        }
        else
        {
            hud.ShowInteraction(
                "Collect all treasures before escaping! (" +
                nearbyPlayer.TreasureCount + "/" +
                requiredTreasures + ")"
            );
        }
    }

    void CompleteHeist()
    {
        hasWon = true;

        Debug.Log("HEIST COMPLETE!");

        if (hud != null)
        {
            hud.ShowVictory();
        }

        PlayerMovement movement =
            nearbyPlayer.GetComponent<PlayerMovement>();

        if (movement != null)
        {
            movement.enabled = false;
        }

        Rigidbody2D rb =
            nearbyPlayer.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}

