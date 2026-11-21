using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private TMP_Text attemptText;
    [SerializeField] private Button restartButton;

    private bool isPaused = false;
    private HeistHUD hud;

    private void Awake()
    {
        // Hide the menu when the game starts
        pausePanel.SetActive(false);
    }

    private void Start()
    {
        hud = FindAnyObjectByType<HeistHUD>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else if (CanPause())
            {
                Pause();
            }
        }
    }

    private bool CanPause()
    {
        HeistSessionManager session =
            HeistSessionManager.Instance;

        if (session == null)
            return false;

        if (session.Outcome !=
            HeistSessionManager.HeistOutcome.Playing)
            return false;

        if (hud != null && hud.IsResultShown)
            return false;

        return true;
    }

    public void Pause()
    {
        if (isPaused || !CanPause())
            return;

        HeistSessionManager session =
            HeistSessionManager.Instance;

        attemptText.text =
            "ATTEMPT " + session.CurrentAttempt +
            " / " + session.MaxAttempts;

        // No more restarts on the final attempt
        restartButton.interactable =
            !session.IsLastAttempt;

        pausePanel.SetActive(true);
        isPaused = true;

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (!isPaused)
            return;

        Time.timeScale = 1f;
        isPaused = false;

        pausePanel.SetActive(false);
    }

    public void RestartAttempt()
    {
        if (!isPaused)
            return;

        HeistSessionManager session =
            HeistSessionManager.Instance;

        if (session == null || session.IsLastAttempt)
            return;

        Time.timeScale = 1f;
        isPaused = false;

        session.RestartAttempt();
    }

    public void AbandonHeist()
    {
        if (!isPaused)
            return;

        Time.timeScale = 1f;
        isPaused = false;

        HeistSessionManager.Instance?.StartNewHeist();
    }

    private void OnDestroy()
    {
        if (isPaused)
            Time.timeScale = 1f;
    }
}

