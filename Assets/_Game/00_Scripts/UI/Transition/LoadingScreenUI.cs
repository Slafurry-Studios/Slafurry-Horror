using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Slafurry.System;
using Slafurry.System.Scene;

namespace Slafurry.UI
{
    /// <summary>
    /// Pure UI observer for LoadingSystem - has zero knowledge of the
    /// boot/late-batch logic itself, only reacts to its events. Should be
    /// a child of the persistent "===SYSTEM===" GameObject so it survives
    /// scene transitions and can react to late-batch loading too (e.g.
    /// Player spawning fresh in a new scene after the initial boot).
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadingScreenUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TMP_Text statusText;

        [Tooltip("Small delay after OnLoadingComplete before hiding, so 100% is visible for a beat instead of vanishing instantly.")]
        [SerializeField] private float hideDelay = 0.3f;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            LoadingSystem.Instance.OnProgressChanged += HandleProgressChanged;
            LoadingSystem.Instance.OnStatusChanged += HandleStatusChanged;
            LoadingSystem.Instance.OnLoadingComplete += HandleLoadingComplete;

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.OnSceneLoadStarted += HandleSceneLoadStarted;
                SceneLoader.Instance.OnSceneLoadProgress += HandleSceneLoadProgress;
                SceneLoader.Instance.OnSceneLoadCompleted += HandleSceneLoadCompleted;
            }

        }

        private void OnDisable()
        {
            if (LoadingSystem.Instance != null)
            {
                LoadingSystem.Instance.OnProgressChanged -= HandleProgressChanged;
                LoadingSystem.Instance.OnStatusChanged -= HandleStatusChanged;
                LoadingSystem.Instance.OnLoadingComplete -= HandleLoadingComplete;
            }

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.OnSceneLoadStarted -= HandleSceneLoadStarted;
                SceneLoader.Instance.OnSceneLoadProgress -= HandleSceneLoadProgress;
                SceneLoader.Instance.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
            }
        }

        private void HandleProgressChanged(float value)
        {
            if (progressSlider != null)
                progressSlider.value = value;

            // any progress update means loading is (re)active - e.g. a
            // late batch just started during a scene transition
            Show();
        }

        private void HandleStatusChanged(string text)
        {
            if (statusText != null)
                statusText.text = text;
        }

        private void HandleLoadingComplete()
        {
            CancelInvoke(nameof(Hide));
            Invoke(nameof(Hide), hideDelay);
        }

        private void HandleSceneLoadStarted(string sceneName)
        {
            statusText.text = $"Loading {sceneName}...";
            Show();
        }

        private void HandleSceneLoadProgress(float progress)
        {
            if (progressSlider != null)
                progressSlider.value = progress;

            Show();
        }

        private void HandleSceneLoadCompleted(string sceneName)
        {
            CancelInvoke(nameof(Hide));
            Invoke(nameof(Hide), hideDelay);
        }

        private void Show()
        {
            CancelInvoke(nameof(Hide));
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        private void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}