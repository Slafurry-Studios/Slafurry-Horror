using System.Collections;
using TMPro;
using UnityEngine;

public class PromptText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text text;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.15f;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private Coroutine showRoutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideImmediate();
    }

    public void Show(string message)
    {
        if (text == null || canvasGroup == null)
            return;

        text.text = message;

        if (showRoutine != null)
            StopCoroutine(showRoutine);

        showRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        gameObject.SetActive(true);

        yield return Fade(0f, 1f, fadeInDuration);

        yield return new WaitForSeconds(displayDuration);

        yield return Fade(1f, 0f, fadeOutDuration);

        gameObject.SetActive(false);

        showRoutine = null;
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
            yield break;
        }

        float elapsed = 0f;

        canvasGroup.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);

            canvasGroup.alpha = Mathf.Lerp(from, to, progress);

            yield return null;
        }

        canvasGroup.alpha = to;
    }

    public void Hide()
    {
        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
            showRoutine = null;
        }

        HideImmediate();
    }

    private void HideImmediate()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
    }
}