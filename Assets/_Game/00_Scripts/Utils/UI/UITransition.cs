using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Slafurry.System.Scene;

namespace Slafurry.Utils.UI
{
    public class UITransition : MonoBehaviour
    {
        [Header("Fade Image")]
        [SerializeField] private Image fadeImage;

        [Header("Transition")]
        [SerializeField] private float fadeDuration = 1f;

        private Coroutine currentRoutine;

        private void Awake()
        {
            if (fadeImage == null)
            {
                Debug.LogError("Fade is not assigned!");
                return;
            }

            fadeImage.gameObject.SetActive(true);
            InstantBlack();
        }

        private void OnEnable()
        {
            if (SceneLoader.Instance == null) return;

            SceneLoader.Instance.OnSceneLoadStarted += HandleSceneLoadStarted;
            SceneLoader.Instance.OnSceneLoadCompleted += HandleSceneLoadCompleted;
        }

        private void OnDisable()
        {
            if (SceneLoader.Instance == null) return;

            SceneLoader.Instance.OnSceneLoadStarted -= HandleSceneLoadStarted;
            SceneLoader.Instance.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
        }

        private IEnumerator Start()
        {
            yield return null;
            FadeOut();
        }

        private void HandleSceneLoadStarted(string sceneName)
        {
            FadeIn();
        }

        private void HandleSceneLoadCompleted(string sceneName)
        {
            FadeOut();
        }

        public void FadeIn() => StartFade(1f);
        public void FadeOut() => StartFade(0f);

        private void StartFade(float targetAlpha)
        {
            if (currentRoutine != null)
            {
                StopCoroutine(currentRoutine);
            }

            currentRoutine = StartCoroutine(FadeRoutine(targetAlpha));
        }

        private IEnumerator FadeRoutine(float targetAlpha)
        {
            fadeImage.gameObject.SetActive(true);

            float startAlpha = fadeImage.color.a;
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                float t = timer / fadeDuration;

                Color color = fadeImage.color;
                color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
                fadeImage.color = color;

                yield return null;
            }

            Color finalColor = fadeImage.color;
            finalColor.a = targetAlpha;
            fadeImage.color = finalColor;

            if (Mathf.Approximately(targetAlpha, 0f))
            {
                fadeImage.gameObject.SetActive(false);
            }

            currentRoutine = null;
        }

        public void InstantBlack()
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 1f;
            fadeImage.color = color;
        }

        public void InstantClear()
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
            fadeImage.gameObject.SetActive(false);
        }
    }
}