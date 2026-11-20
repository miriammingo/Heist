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
    [SerializeField] private TMP_Text resultButtonText;

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

        if (resultTitle != null)
            resultTitle.text = "HEIST COMPLETE";

        if (resultSubtitle != null)
            resultSubtitle.text =
                "You escaped with all the treasures!";

        if (resultButtonText != null)
            resultButtonText.text = "NEW HEIST";

        if (winPanel != null)
            winPanel.SetActive(true);
    }


    // NEW: fail state. Reuses the result panel with a different message.

    public void ShowCaught()
    {
        HideInteraction();

        HeistSessionManager session =
            HeistSessionManager.Instance;

        bool finalAttempt =
            session != null && session.IsLastAttempt;

        if (resultTitle != null)
        {
            resultTitle.text = finalAttempt
                ? "HEIST FAILED!"
                : "CAUGHT!";
        }

        if (resultSubtitle != null)
        {
            resultSubtitle.text = finalAttempt
                ? "No attempts left. The museum wins."
                : "All treasures have been confiscated. Try again!";
        }

        if (resultButtonText != null)
        {
            resultButtonText.text = finalAttempt
                ? "NEW HEIST"
                : "TRY AGAIN";
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }


    public void RestartGame()
    {
        HeistSessionManager session =
            HeistSessionManager.Instance;

        if (session != null)
        {
            session.ContinueAfterResult();
        }
        else
        {
            // Fallback for test scenes without the manager.
            Time.timeScale = 1f;

            SceneManager.LoadScene(
                SceneManager.GetActiveScene().name
            );
        }
    }

}

