using UnityEngine;

public class TreasurePickup : MonoBehaviour
{
    [SerializeField] private string treasureName = "Starlight Diamond";

    private PlayerTreasureInventory nearbyPlayer;
    private HeistHUD hud;

    private void Start()
    {
        hud = FindAnyObjectByType<HeistHUD>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerTreasureInventory inventory =
            other.GetComponent<PlayerTreasureInventory>();

        if (inventory != null)
        {
            nearbyPlayer = inventory;

            if (hud != null)
            {
                hud.ShowInteraction(
                    "Press [E] to steal the " + treasureName
                );
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerTreasureInventory inventory =
            other.GetComponent<PlayerTreasureInventory>();

        if (inventory != null && inventory == nearbyPlayer)
        {
            nearbyPlayer = null;

            if (hud != null)
            {
                hud.HideInteraction();
            }
        }
    }

    private void Update()
    {
        if (nearbyPlayer != null &&
            Input.GetKeyDown(KeyCode.E))
        {
            nearbyPlayer.CollectTreasure(treasureName);

            if (hud != null)
            {
                hud.SetTreasureCount(nearbyPlayer.TreasureCount);
                hud.HideInteraction();
            }

            nearbyPlayer = null;
            Destroy(gameObject);
        }
    }
}
