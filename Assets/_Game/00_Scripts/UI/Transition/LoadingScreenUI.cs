using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Slafurry.System;
using Slafurry.System.Scene;

namespace Slafurry.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class LoadingScreenUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private TMP_Text statusText;

        [SerializeField] private float hideDelay = 0.3f;

        [SerializeField] private string[] excludedScenes;

        private string currentLoadingScene;
        private bool isCurrentSceneExcluded;

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

        private bool IsSceneExcluded(string sceneName)
        {
            if (excludedScenes == null || excludedScenes.Length == 0)
                return false;

            for (int i = 0; i < excludedScenes.Length; i++)
            {
                if (string.Equals(excludedScenes[i], sceneName, global::System.StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

 
        private bool IsActiveSceneExcluded()
        {
            return IsSceneExcluded(SceneManager.GetActiveScene().name);
        }

        private void HandleProgressChanged(float value)
        {
            if (IsActiveSceneExcluded())
                return;

            if (progressSlider != null)
                progressSlider.value = value;

            Show();
        }

        private void HandleStatusChanged(string text)
        {
            if (IsActiveSceneExcluded())
                return;

            if (statusText != null)
                statusText.text = text;
        }

        private void HandleLoadingComplete()
        {
            if (IsActiveSceneExcluded())
                return;

            CancelInvoke(nameof(Hide));
            Invoke(nameof(Hide), hideDelay);
        }

        private void HandleSceneLoadStarted(string sceneName)
        {
            currentLoadingScene = sceneName;
            isCurrentSceneExcluded = IsSceneExcluded(sceneName);

            if (isCurrentSceneExcluded)
                return;

            statusText.text = $"Loading {sceneName}...";
            Show();
        }

        private void HandleSceneLoadProgress(float progress)
        {
            if (isCurrentSceneExcluded)
                return;

            if (progressSlider != null)
                progressSlider.value = progress;

            Show();
        }

        private void HandleSceneLoadCompleted(string sceneName)
        {
            bool wasExcluded = isCurrentSceneExcluded;

            currentLoadingScene = null;
            isCurrentSceneExcluded = false;

            if (wasExcluded)
                return;

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