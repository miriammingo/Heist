using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class HeistHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text interactionText;
    [SerializeField] private TMP_Text treasureCounter;
    [SerializeField] private GameObject winPanel;

    // NEW: the texts of the result panel, so the same panel can also say "CAUGHT!".
    [SerializeField] private TMP_Text resultTitle;
    [SerializeField] private TMP_Text resultSubtitle;

    // NEW: true while the win/lose panel is on screen.
    public bool IsResultShown => winPanel != null && winPanel.activeSelf;

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

    // NEW: fail state. Reuses the result panel with a different message.
    public void ShowCaught()
    {
        HideInteraction();

        if (resultTitle != null)
        {
            resultTitle.text = "CAUGHT!";
        }

        if (resultSubtitle != null)
        {
            resultSubtitle.text = "A guard got you. Try a different route!";
        }

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

