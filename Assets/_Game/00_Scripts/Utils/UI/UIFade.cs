using System;
using System.Collections;
using UnityEngine;

namespace Slafurry.Utils.UI
{
    /// <summary>
    /// Simple one-shot fade in/out for a CanvasGroup. Use for screen
    /// transitions, tooltips appearing/disappearing, or any UI element
    /// that needs to fade rather than blink (see UIBlink for repeating loop).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIFade : MonoBehaviour
    {
        [SerializeField] private CanvasGroup[] canvasGroups;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool disableInteractionWhileHidden = true;

        public event Action OnFadeInComplete;
        public event Action OnFadeOutComplete;

        private Coroutine _routine;

        private void Awake()
        {
            if (canvasGroups == null)
                canvasGroups = GetComponents<CanvasGroup>();
        }

        public void FadeIn(float? overrideDuration = null)
        {
            foreach (var canvasGroup in canvasGroups)
            {
                StartFade(canvasGroup.alpha, 1f, overrideDuration ?? duration, () => OnFadeInComplete?.Invoke());
            }
        }

        public void FadeOut(float? overrideDuration = null)
        {
            foreach (var canvasGroup in canvasGroups)
            {
                StartFade(canvasGroup.alpha, 0f, overrideDuration ?? duration, () => OnFadeOutComplete?.Invoke());
            }
        }

        public void SetImmediate(float alpha)
        {
            if (_routine != null) StopCoroutine(_routine);
            ApplyAlpha(alpha);
        }

        private void StartFade(float from, float to, float dur, Action onComplete)
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(FadeRoutine(from, to, dur, onComplete));
        }

        private IEnumerator FadeRoutine(float from, float to, float dur, Action onComplete)
        {
            float t = 0f;
            while (t < dur)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                ApplyAlpha(Mathf.Lerp(from, to, dur > 0 ? t / dur : 1f));
                yield return null;
            }
            ApplyAlpha(to);
            _routine = null;
            onComplete?.Invoke();
        }

        private void ApplyAlpha(float alpha)
        {
            foreach (var canvasGroup in canvasGroups)
            {
                canvasGroup.alpha = alpha;
                if (disableInteractionWhileHidden)
                {
                    bool visible = alpha > 0.01f;
                    canvasGroup.interactable = visible;
                    canvasGroup.blocksRaycasts = visible;
                }
            }

        }
    }
}