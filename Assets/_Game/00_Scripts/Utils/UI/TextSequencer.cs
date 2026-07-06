using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Slafurry.System.Audio;
using Slafurry.System.Scene;

public class TextSequencer : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string sequenceId;

    [Header("Reference")]
    [SerializeField] private TMP_Text textComponent;

    [Header("Content")]
    [TextArea(1, 3)]
    [SerializeField] private string[] texts;

    [Header("Typewriter")]
    [SerializeField] private bool useTypewriter = true;
    [SerializeField] private float charInterval = 0.03f;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float fadeOutDuration = 0.6f;

    [Header("Timing")]
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private float delayBeforeStart = 0f;

    [Header("SFX")]
    [SerializeField] private string sfxId = "Type_Effect";

    [Header("Options")]
    [SerializeField] private bool loop = true;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool allowSkip = true;

    [Header("Events")]
    public UnityEvent onTextEnd;
    public UnityEvent onSequenceEnd;

    private Coroutine _routine;

    private void Start()
    {
        if (!playOnStart) return;

        if (HasBeenSeen())
        {
            SetAlpha(0f);
            onSequenceEnd?.Invoke();
            return;
        }

        Play();
    }

    private void Update()
    {
        if (!allowSkip) return;
        if (_routine == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SkipCutscene();
        }
    }

    public void SkipCutscene()
    {
        Stop();
        SetAlpha(0f);
        MarkAsSeen();
        onSequenceEnd?.Invoke();
    }

    public void Play()
    {
        if (HasBeenSeen())
        {
            SetAlpha(0f);
            onSequenceEnd?.Invoke();
            return;
        }

        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(FadeRoutine());
    }

    public void Stop()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        StopTypeSfx();
    }

    public void ChangeScene(string newScene)
    {
        Stop();
        SceneSystem.Load(newScene);
    }

    private IEnumerator FadeRoutine()
    {
        if (textComponent == null || texts == null || texts.Length == 0)
        {
            Debug.LogWarning("TMPFadeArray: textComponent or texts array is not set.");
            yield break;
        }

        if (delayBeforeStart > 0f)
            yield return new WaitForSeconds(delayBeforeStart);

        int index = 0;

        do
        {
            textComponent.text = texts[index];
            textComponent.ForceMeshUpdate();

            int totalChars = textComponent.textInfo.characterCount;
            textComponent.maxVisibleCharacters = useTypewriter ? 0 : totalChars;
            SetAlpha(0f);

            yield return FadeTo(0f, 1f, fadeInDuration);

            if (useTypewriter)
                yield return TypeText(totalChars);

            yield return new WaitForSeconds(holdDuration);

            yield return FadeTo(1f, 0f, fadeOutDuration);

            onTextEnd?.Invoke();

            index++;
            if (index >= texts.Length)
                index = 0;

        } while (loop || index != 0);

        MarkAsSeen();

        onSequenceEnd?.Invoke();
        _routine = null;
    }

    private IEnumerator TypeText(int totalChars)
    {
        PlayTypeSfx();

        int visible = 0;
        while (visible < totalChars)
        {
            visible++;
            textComponent.maxVisibleCharacters = visible;
            yield return new WaitForSeconds(charInterval);
        }

        StopTypeSfx();
    }

    private void PlayTypeSfx()
    {
        Audio.PlaySFX2D("UI", sfxId, loop: true);
    }

    private void StopTypeSfx()
    {
        Audio.StopSFX("UI", sfxId);
    }

    private IEnumerator FadeTo(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, t / duration);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(to);
    }

    private void SetAlpha(float alpha)
    {
        Color c = textComponent.color;
        c.a = alpha;
        textComponent.color = c;
    }

    private bool HasBeenSeen()
    {
        if (string.IsNullOrEmpty(sequenceId))
            return false;

        return PlayerPrefs.GetInt("tmpfade_" + sequenceId, 0) == 1;
    }

    private void MarkAsSeen()
    {
        if (string.IsNullOrEmpty(sequenceId))
            return;

        PlayerPrefs.SetInt("tmpfade_" + sequenceId, 1);
        PlayerPrefs.Save();
    }
}