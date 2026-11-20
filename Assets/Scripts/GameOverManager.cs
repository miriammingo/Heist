using UnityEngine;

/// <summary>
/// FAIL STATE: ends the game when a guard catches the thief.
/// It LISTENS to the guards' ThiefCaught event (Observer pattern): the guards don't need
/// to know who handles the game over, they just announce "caught!".
/// Put it on an empty object called "GameOverManager" in the Museum scene.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    private HeistHUD hud;
    private bool isOver;

    private void Awake()
    {
        hud = FindAnyObjectByType<HeistHUD>();
    }

    // Start listening when this object is active, stop when it isn't.
    private void OnEnable()
    {
        GuardAI.ThiefCaught += HandleThiefCaught;
    }

    private void OnDisable()
    {
        GuardAI.ThiefCaught -= HandleThiefCaught;
    }

    private void HandleThiefCaught()
    {
        // Ignore it if the game has already ended (caught twice, or the thief had already escaped).
        if (isOver || (hud != null && hud.IsResultShown))
        {
            return;
        }

        isOver = true;

        HeistSessionManager.Instance?.MarkCaught();

        Debug.Log("CAUGHT!");
        StopThief();
        StopGuards();


        if (hud != null)
        {
            hud.ShowCaught();
        }
    }

    private static void StopThief()
    {
        PlayerMovement thief = FindAnyObjectByType<PlayerMovement>();
        if (thief == null)
        {
            return;
        }

        thief.enabled = false;
        if (thief.TryGetComponent(out Rigidbody2D body))
        {
            body.linearVelocity = Vector2.zero;
        }
    }

    private static void StopGuards()
    {
        foreach (GuardAI guard in FindObjectsByType<GuardAI>())
        {
            guard.enabled = false;
        }
    }
}
