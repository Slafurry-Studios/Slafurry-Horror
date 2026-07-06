using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Slafurry.Core.Abstract;

namespace Slafurry.System.Scene
{
    public static class SceneSystem
    {
        public static void Load(string sceneName) => SceneLoader.Instance.LoadScene(sceneName);
    }
    
    public class SceneLoader : GameSystem<SceneLoader>
    {
        public event Action<string> OnSceneLoadStarted;
        public event Action<float> OnSceneLoadProgress;
        public event Action<string> OnSceneLoadCompleted;

        private bool _isLoading;

        public override IEnumerator Initialize() { yield return null; }
        public override void PostInitialize() { }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
        }

        public void LoadScene(string sceneName)
        {
            if (_isLoading)
            {
                Debug.LogWarning($"[SceneLoader] Already loading a scene, ignoring request for '{sceneName}'");
                return;
            }
            StartCoroutine(LoadRoutine(sceneName));
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            _isLoading = true;
            OnSceneLoadStarted?.Invoke(sceneName);

            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                OnSceneLoadProgress?.Invoke(operation.progress / 0.9f);
                yield return null;
            }

            OnSceneLoadProgress?.Invoke(1f);
            operation.allowSceneActivation = true;

            yield return operation;

            _isLoading = false;
            OnSceneLoadCompleted?.Invoke(sceneName);
        }
    }
}