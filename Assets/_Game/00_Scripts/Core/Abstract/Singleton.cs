using System.Collections;
using Slafurry.Core.Interface;
using Slafurry.System;
using UnityEngine;

namespace Slafurry.Core.Abstract
{
    public abstract class Singleton<T> : MonoBehaviour, IInitializable
        where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        public virtual int Priority => 0;

        // Sealed (not virtual) - override subclass from OnSingletonAwake(),
        // so register to LoadingManager never forgot to be called.
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this as T;
            OnSingletonAwake();
        }

        private void Start()
        {
            LoadingSystem.Instance.Register(this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            OnSingletonDestroyed();
        }

        /// <summary>Hook Awake untuk subclass. GameSystem&lt;T&gt; pakai ini buat DontDestroyOnLoad.</summary>
        protected abstract void OnSingletonAwake();

        /// <summary>Hook OnDestroy opsional untuk subclass.</summary>
        protected virtual void OnSingletonDestroyed() { }

        public abstract IEnumerator Initialize();
        public abstract void PostInitialize();
    }
}
