using UnityEngine;

public class MuseumRoomZone : MonoBehaviour
{
    [SerializeField] private string roomId = "Gallery_A";

    private bool playerInside = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerTreasureInventory player =
            other.GetComponentInParent<PlayerTreasureInventory>();

        if (player == null || playerInside)
            return;

        playerInside = true;

        HeistSessionManager.Instance?.RegisterRoomEntry(roomId);
    }

    private void Update()
    {
        if (!playerInside)
            return;

        HeistSessionManager session =
            HeistSessionManager.Instance;

        if (session != null)
        {
            session.AddRoomTime(roomId, Time.deltaTime);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerTreasureInventory player =
            other.GetComponentInParent<PlayerTreasureInventory>();

        if (player != null)
        {
            playerInside = false;
        }
    }
}

