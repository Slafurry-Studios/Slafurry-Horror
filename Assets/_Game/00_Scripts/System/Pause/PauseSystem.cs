using System;
using System.Collections;
using UnityEngine;
using Slafurry.Core.Abstract;

namespace Slafurry.System.Pause
{
    public static class Pause
    {
        public static void On() => PauseSystem.Instance.Pause();
        public static void Off() => PauseSystem.Instance.Resume();
        public static void Toggle() => PauseSystem.Instance.Toggle();
        public static bool IsPaused => PauseSystem.Instance.IsPaused;
    }
    
    public class PauseSystem : GameSystem<PauseSystem>
    {
        public bool IsPaused { get; private set; }

        public event Action OnPaused;
        public event Action OnResumed;

        public override IEnumerator Initialize() { yield return null; }
        public override void PostInitialize() { }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake();
        }

        public void Pause()
        {
            if (IsPaused) return;
            IsPaused = true;
            Time.timeScale = 0f;
            OnPaused?.Invoke();
        }

        public void Resume()
        {
            if (!IsPaused) return;
            IsPaused = false;
            Time.timeScale = 1f;
            OnResumed?.Invoke();
        }

        public void Toggle()
        {
            if (IsPaused) Resume();
            else Pause();
        }
    }
}