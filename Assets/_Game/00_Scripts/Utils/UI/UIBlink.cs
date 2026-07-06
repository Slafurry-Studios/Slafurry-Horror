using System.Collections;
using UnityEngine;

namespace Slafurry.Utils.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIBlink : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Blink Settings")]
        [SerializeField] private float minAlpha = 0.2f;
        [SerializeField] private float maxAlpha = 1f;
        [SerializeField] private float blinkSpeed = 2f;

        [Header("Options")]
        [SerializeField] private bool infinite = true;
        [SerializeField] private int blinkCount = 3;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool disableInteractionWhileDim = false;

        private Coroutine _routine;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (playOnEnable)
                Play();
        }

        private void OnDisable()
        {
            Stop();
        }

        public void Play()
        {
            if (_routine != null)
                StopCoroutine(_routine);
            _routine = StartCoroutine(infinite ? BlinkInfinite() : BlinkFixedCount());
        }

        public void Stop()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
            canvasGroup.alpha = maxAlpha;
        }

        private IEnumerator BlinkInfinite()
        {
            float t = 0f;
            while (true)
            {
                t += GetDeltaTime() * blinkSpeed;
                float wave = (Mathf.Sin(t) + 1f) * 0.5f;
                float alpha = Mathf.Lerp(minAlpha, maxAlpha, wave);
                SetAlpha(alpha);
                yield return null;
            }
        }

        private IEnumerator BlinkFixedCount()
        {
            for (int i = 0; i < blinkCount; i++)
            {
                yield return FadeAlpha(maxAlpha, minAlpha);
                yield return FadeAlpha(minAlpha, maxAlpha);
            }
            canvasGroup.alpha = maxAlpha;
            _routine = null;
        }

        private IEnumerator FadeAlpha(float from, float to)
        {
            float duration = 1f / blinkSpeed;
            float t = 0f;
            while (t < duration)
            {
                t += GetDeltaTime();
                float alpha = Mathf.Lerp(from, to, t / duration);
                SetAlpha(alpha);
                yield return null;
            }
            SetAlpha(to);
        }

        private void SetAlpha(float alpha)
        {
            canvasGroup.alpha = alpha;
            if (disableInteractionWhileDim)
            {
                bool isVisibleEnough = alpha > 0.5f;
                canvasGroup.interactable = isVisibleEnough;
                canvasGroup.blocksRaycasts = isVisibleEnough;
            }
        }

        private float GetDeltaTime()
        {
            return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }
    }
}