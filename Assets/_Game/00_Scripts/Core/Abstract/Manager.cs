using System.Collections;
using Slafurry.Core.Interface;
using Slafurry.System;
using UnityEngine;

namespace Slafurry.Core.Abstract
{
    /// <summary>
    /// Base for all "...Manager" - session/scene-scoped gameplay
    /// coordinators (EnemyManager, WaveManager, BulletManager). NOT a
    /// standalone singleton; registers itself with GameManager as the
    /// single global access point. This is also the "public front door" -
    /// other programmers only call this Manager's public methods without
    /// needing to know its internals.
    ///
    /// IMPORTANT: RegisterToGameManager() runs during PostInitialize(),
    /// NOT Awake() - because it touches GameManager.Instance, and Unity
    /// never guarantees Awake() order across different GameObjects.
    /// By the time PostInitialize() runs (triggered from LoadingSystem's
    /// Start(), which only fires after every Awake() in the scene has
    /// completed), GameManager.Instance is guaranteed to be set.
    /// </summary>
    public abstract class Manager : MonoBehaviour, IInitializable
    {
        public virtual int Priority => 0;

        // Sealed - subclasses hook in via OnManagerAwake(), so
        // LoadingSystem.Register() is never forgotten and Awake() never
        // accidentally touches another object.
        private void Awake()
        {
            LoadingSystem.Instance.Register(this);
            OnManagerAwake();
        }

        private void OnDestroy()
        {
            UnregisterFromGameManager();
            OnManagerDestroyed();
        }

        /// <summary>Additional Awake hook for subclasses. Must NOT touch other objects here.</summary>
        protected virtual void OnManagerAwake() { }

        /// <summary>Additional OnDestroy hook for subclasses.</summary>
        protected virtual void OnManagerDestroyed() { }

        /// <summary>Required, e.g.: GameManager.Instance.RegisterEnemyManager(this)</summary>
        protected abstract void RegisterToGameManager();

        /// <summary>Required, e.g.: GameManager.Instance.UnregisterEnemyManager(this)</summary>
        protected abstract void UnregisterFromGameManager();

        public abstract IEnumerator Initialize();

        // Sealed - guarantees RegisterToGameManager() always runs first,
        // safely, before the subclass's own PostInitialize logic.
        // Subclasses override OnPostInitialize() instead of PostInitialize().
        public void PostInitialize()
        {
            RegisterToGameManager();
            OnPostInitialize();
        }

        /// <summary>Implement your own PostInitialize logic here instead of overriding PostInitialize() directly.</summary>
        protected abstract void OnPostInitialize();
    }
}