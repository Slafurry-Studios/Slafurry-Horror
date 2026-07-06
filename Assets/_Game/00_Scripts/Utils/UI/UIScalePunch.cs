using System.Collections;
using UnityEngine;

namespace Slafurry.Utils.UI
{
    /// <summary>
    /// One-shot "punch" scale effect for UI elements - buttons that bounce
    /// on click, notifications that pop in, etc. Unlike GameFeel's
    /// SquashStretch (which targets gameplay Transforms), this is built
    /// for RectTransform / UI scale space.
    /// </summary>
    public class UIScalePunch : MonoBehaviour
    {
        [SerializeField] private RectTransform target;
        [SerializeField] private float punchScale = 1.2f;
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private bool useUnscaledTime = true;

        private Vector3 _originalScale;
        private Coroutine _routine;

        private void Awake()
        {
            if (target == null) target = transform as RectTransform;
            _originalScale = target.localScale;
        }

        public void Play()
        {
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(PunchRoutine());
        }

        private IEnumerator PunchRoutine()
        {
            Vector3 peak = _originalScale * punchScale;
            float half = duration * 0.5f;

            yield return Scale(_originalScale, peak, half);
            yield return Scale(peak, _originalScale, half);

            target.localScale = _originalScale;
            _routine = null;
        }

        private IEnumerator Scale(Vector3 from, Vector3 to, float dur)
        {
            float t = 0f;
            while (t < dur)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float eased = curve.Evaluate(Mathf.Clamp01(t / dur));
                target.localScale = Vector3.LerpUnclamped(from, to, eased);
                yield return null;
            }
        }
    }
}