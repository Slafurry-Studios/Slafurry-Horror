using System;
using System.Collections;
using UnityEngine;

namespace Slafurry.Utils.UI
{
    /// <summary>
    /// Slides a UI element in from off-screen and out again, from any of
    /// the 4 edges. Call SlideIn() / SlideOut() manually, or leave
    /// playOnEnable checked to auto slide-in whenever the GameObject
    /// becomes active (e.g. right after UIManager.ShowScreen()).
    /// </summary>
    public class UISlide : MonoBehaviour
    {
        public enum Direction { Top, Bottom, Left, Right }

        [Header("Reference")]
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RectTransform canvasRect;

        [Header("Direction")]
        [SerializeField] private Direction fromDirection = Direction.Left;
        [SerializeField] private float extraOffset = 100f;

        [Header("Timing")]
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float delay = 0f;
        [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Options")]
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool deactivateOnHidden = false;

        /// <summary>Fired when a SlideOut() finishes (e.g. safe point to deactivate the panel).</summary>
        public event Action OnSlideOutComplete;

        /// <summary>Fired when a SlideIn() finishes.</summary>
        public event Action OnSlideInComplete;

        private Vector2 _shownPos;   // the resting/visible position, captured once at Awake
        private Vector2 _hiddenPos;  // the off-screen position, computed from Direction
        private Coroutine _routine;

        private void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (canvasRect == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null)
                    canvasRect = canvas.rootCanvas.transform as RectTransform;
            }

            _shownPos = rectTransform.anchoredPosition;
            _hiddenPos = CalculateHiddenPos();
        }

        private void OnEnable()
        {
            if (playOnEnable)
                SlideIn();
        }

        public void SlideIn()
        {
            rectTransform.anchoredPosition = _hiddenPos;
            StartSlide(_hiddenPos, _shownPos, onComplete: () => OnSlideInComplete?.Invoke());
        }

        public void SlideOut()
        {
            StartSlide(rectTransform.anchoredPosition, _hiddenPos, onComplete: () =>
            {
                OnSlideOutComplete?.Invoke();
                if (deactivateOnHidden)
                    gameObject.SetActive(false);
            });
        }

        private void StartSlide(Vector2 from, Vector2 to, Action onComplete)
        {
            if (_routine != null)
                StopCoroutine(_routine);
            _routine = StartCoroutine(SlideRoutine(from, to, onComplete));
        }

        private Vector2 CalculateHiddenPos()
        {
            float ownWidth = rectTransform.rect.width;
            float ownHeight = rectTransform.rect.height;
            float screenWidth = canvasRect != null ? canvasRect.rect.width : Screen.width;
            float screenHeight = canvasRect != null ? canvasRect.rect.height : Screen.height;

            float horizontalDistance = (screenWidth * 0.5f) + (ownWidth * 0.5f) + extraOffset;
            float verticalDistance = (screenHeight * 0.5f) + (ownHeight * 0.5f) + extraOffset;

            return fromDirection switch
            {
                Direction.Top => _shownPos + new Vector2(0f, verticalDistance),
                Direction.Bottom => _shownPos - new Vector2(0f, verticalDistance),
                Direction.Left => _shownPos - new Vector2(horizontalDistance, 0f),
                Direction.Right => _shownPos + new Vector2(horizontalDistance, 0f),
                _ => _shownPos
            };
        }

        private IEnumerator SlideRoutine(Vector2 from, Vector2 to, Action onComplete)
        {
            if (delay > 0f)
                yield return useUnscaledTime ? new WaitForSecondsRealtime(delay) : new WaitForSeconds(delay);

            float t = 0f;
            while (t < duration)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float normalized = Mathf.Clamp01(t / duration);
                float eased = easeCurve.Evaluate(normalized);
                rectTransform.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
                yield return null;
            }

            rectTransform.anchoredPosition = to;
            _routine = null;
            onComplete?.Invoke();
        }
    }
}