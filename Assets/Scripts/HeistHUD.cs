using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HeistHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text interactionText;
    [SerializeField] private TMP_Text treasureCounter;
    [SerializeField] private GameObject winPanel;

    private void Awake()
    {
        HideInteraction();
        SetTreasureCount(0);

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    public void ShowInteraction(string message)
    {
        interactionText.text = message;
        interactionText.gameObject.SetActive(true);
    }

    public void HideInteraction()
    {
        interactionText.gameObject.SetActive(false);
    }

    public void SetTreasureCount(int count)
    {
        treasureCounter.text = "Treasures found: " + count;
    }

    public void ShowVictory()
    {
        HideInteraction();

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
}

