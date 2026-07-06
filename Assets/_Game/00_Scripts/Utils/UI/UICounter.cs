using System.Collections;
using UnityEngine;
using TMPro;

namespace Slafurry.Utils.UI
{
    /// <summary>
    /// Animates a displayed number smoothly toward a target value instead
    /// of snapping instantly. Use for score, currency, HP text, etc.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class UICounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private string format = "N0"; // e.g. "N0" -> 1,500
        [SerializeField] private bool useUnscaledTime = true;

        private float _currentValue;
        private Coroutine _routine;

        private void Awake()
        {
            if (label == null) label = GetComponent<TMP_Text>();
        }

        /// <summary>Jump straight to a value with no animation - use for initial setup.</summary>
        public void SetImmediate(float value)
        {
            if (_routine != null) StopCoroutine(_routine);
            _currentValue = value;
            label.text = value.ToString(format);
        }

        /// <summary>Animate from the current displayed value to a new target.</summary>
        public void AnimateTo(float target, float? overrideDuration = null)
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(CountRoutine(target, overrideDuration ?? duration));
        }

        private IEnumerator CountRoutine(float target, float dur)
        {
            float start = _currentValue;
            float t = 0f;

            while (t < dur)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                _currentValue = Mathf.Lerp(start, target, Mathf.Clamp01(t / dur));
                label.text = _currentValue.ToString(format);
                yield return null;
            }

            _currentValue = target;
            label.text = _currentValue.ToString(format);
            _routine = null;
        }
    }
}