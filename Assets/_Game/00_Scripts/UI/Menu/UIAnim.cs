using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIAnim : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image targetImage;

    [Header("Frames")]
    [SerializeField] private Sprite[] frames;

    [Header("Settings")]
    [SerializeField] private float fps = 12f;
    [SerializeField] private bool loop = false;

    private Coroutine animationCoroutine;

    void OnEnable()
    {
        StartCoroutine(DelayedStart());
    }

    void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    IEnumerator DelayedStart()
    {
        yield return null;

        Debug.Log($"[GlitchAnim] frames={frames?.Length}, targetImage={targetImage}, fps={fps}");

        if (frames == null || frames.Length == 0 || targetImage == null)
        {
            Debug.LogWarning("[GlitchAnim] Gagal start — cek referensi di Inspector!");
            yield break;
        }

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        animationCoroutine = StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        Debug.Log("[GlitchAnim] PlayAnimation mulai");

        float interval = 1f / Mathf.Max(fps, 0.01f);

        do
        {
            for (int i = 0; i < frames.Length; i++)
            {
                Debug.Log($"[GlitchAnim] Frame {i} → {frames[i]?.name}");
                targetImage.sprite = frames[i];
                yield return new WaitForSeconds(interval);
            }
        }
        while (loop);

        Debug.Log("[GlitchAnim] PlayAnimation selesai");
        animationCoroutine = null;
    }
}