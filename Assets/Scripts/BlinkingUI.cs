using UnityEngine;

/// <summary>
/// Makes a UI element blink while it is active (used for the HIGH ALERT banner).
/// It fades the element in and out using a CanvasGroup.
/// </summary>
public class BlinkingUI : MonoBehaviour
{
    [Tooltip("How many times per second it blinks.")]
    [SerializeField] private float blinksPerSecond = 2f;

    [Tooltip("How transparent it gets at its faintest (0 = invisible, 1 = no blinking).")]
    [SerializeField, Range(0f, 1f)] private float minimumAlpha = 0.3f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Update()
    {
        // PingPong goes 0 -> 1 -> 0 -> 1... over time.
        float wave = Mathf.PingPong(Time.unscaledTime * blinksPerSecond * 2f, 1f);
        canvasGroup.alpha = Mathf.Lerp(minimumAlpha, 1f, wave);
    }
}
