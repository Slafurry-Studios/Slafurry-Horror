using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Slafurry.Core.Abstract;

namespace Slafurry.Utils.GameFeel
{
    /// <summary>
    /// Full-screen color overlay flash (e.g. red flash on taking damage).
    /// LocalSingleton like CameraShake - one flash overlay per scene.
    /// Self-configured via Inspector since PlayEffect() takes no parameters;
    /// use SetColor()/SetDuration() before PlayEffect() if you need a
    /// different flash color for a specific call (e.g. heal = green flash).
    /// </summary>
    public class ScreenFlash : LocalSingleton<ScreenFlash>, IGameFeelEffect
    {
        [SerializeField] private Image overlayImage;
        [SerializeField] private Color color = new Color(1f, 0f, 0f, 0.4f);
        [SerializeField] private float duration = 0.2f;

        private Coroutine _routine;

        protected override void OnSingletonAwake()
        {
            if (overlayImage != null)
            {
                var c = overlayImage.color;
                c.a = 0f;
                overlayImage.color = c;
            }
        }

        public override IEnumerator Initialize() { yield return null; }
        public override void PostInitialize() { }

        /// <summary>Optional override before calling PlayEffect() - e.g. GameFeel.Flash uses this.</summary>
        public void SetColor(Color newColor) => color = newColor;
        public void SetDuration(float newDuration) => duration = newDuration;

        public void PlayEffect()
        {
            if (overlayImage == null) return;

            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(FlashRoutine(color, duration));
        }

        public void StopEffect()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
            if (overlayImage != null)
            {
                var c = overlayImage.color;
                c.a = 0f;
                overlayImage.color = c;
            }
        }

        private IEnumerator FlashRoutine(Color flashColor, float flashDuration)
        {
            overlayImage.color = flashColor;

            float t = 0f;
            while (t < flashDuration)
            {
                t += Time.unscaledDeltaTime;
                var c = flashColor;
                c.a = Mathf.Lerp(flashColor.a, 0f, t / flashDuration);
                overlayImage.color = c;
                yield return null;
            }

            var final = flashColor;
            final.a = 0f;
            overlayImage.color = final;
            _routine = null;
        }
    }
}