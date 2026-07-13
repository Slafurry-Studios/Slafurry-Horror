using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Slafurry.Core.Abstract;

namespace Slafurry.System.Pause
{
    public static class Pause
    {
        public static void On(string key = "Global") => PauseSystem.Instance.Pause(key);
        public static void Off(string key = "Global") => PauseSystem.Instance.Resume(key);
        public static void Toggle(string key = "Global") => PauseSystem.Instance.Toggle(key);

        public static bool IsPaused => PauseSystem.Instance.IsPaused;
        public static bool IsPausedBy(string key) => PauseSystem.Instance.IsPausedBy(key);
        public static void ForceResume() => PauseSystem.Instance.ForceResumeAll();
    }

    public class PauseSystem : GameSystem<PauseSystem>
    {
        private readonly HashSet<string> _pauseStack = new HashSet<string>();

        public bool IsPaused => _pauseStack.Count > 0;

        public event Action OnPaused;
        public event Action OnResumed;
        public event Action<string> OnPauseRequested;   
        public event Action<string> OnPauseReleased;

        public override IEnumerator Initialize() { yield return null; }
        public override void PostInitialize() { }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
        }


        public void Pause(string key = "Global")
        {
            bool wasPaused = IsPaused;

            if (_pauseStack.Add(key))
            {
                OnPauseRequested?.Invoke(key);
            }

            ApplyState(wasPaused);
        }

        public void Resume(string key = "Global")
        {
            bool wasPaused = IsPaused;

            if (_pauseStack.Remove(key))
            {
                OnPauseReleased?.Invoke(key);
            }

            ApplyState(wasPaused);
        }

        public void Toggle(string key = "Global")
        {
            if (IsPausedBy(key)) Resume(key);
            else Pause(key);
        }

        public bool IsPausedBy(string key) => _pauseStack.Contains(key);

        public void ForceResumeAll()
        {
            bool wasPaused = IsPaused;
            _pauseStack.Clear();
            ApplyState(wasPaused);
        }

        private void ApplyState(bool wasPaused)
        {
            bool nowPaused = IsPaused;
            if (nowPaused == wasPaused) return; 
            
            Time.timeScale = nowPaused ? 0f : 1f;

            if (nowPaused) OnPaused?.Invoke();
            else OnResumed?.Invoke();
        }
    }
}