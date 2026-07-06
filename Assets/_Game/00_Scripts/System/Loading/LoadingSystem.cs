using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Slafurry.Core.Interface;
using Slafurry.Core.Abstract;

namespace Slafurry.System
{
    /// <summary>
    /// Pure logic loading sequence. Fires events for progress/status,
    /// does not know about any UI implementation.
    ///
    /// IMPORTANT: the initial boot sequence (LoadSequence) only runs once.
    /// Any object that registers AFTER that has already finished (e.g. a
    /// Player prefab that only exists in GameScene, spawned after boot
    /// already completed back in an earlier scene like IntroCutscene) is
    /// caught here and processed as its own "late batch" - one frame
    /// later, so Initialize()-before-PostInitialize() ordering is still
    /// honored among objects that arrive together in the same scene load.
    /// </summary>
    public class LoadingSystem : GameSystem<LoadingSystem>
    {
        [SerializeField] private float perObjectTimeoutSeconds = 10f;

        private readonly List<IInitializable> _registered = new();
        private readonly List<IInitializable> _pendingLate = new();
        private bool _snapshotTaken;
        private Coroutine _lateBatchRoutine;

        public event Action<float> OnProgressChanged;
        public event Action<string> OnStatusChanged;
        public event Action OnLoadingComplete;

        public void Register(IInitializable obj)
        {
            _registered.Add(obj);

            if (_snapshotTaken)
            {
                _pendingLate.Add(obj);
                if (_lateBatchRoutine == null)
                    _lateBatchRoutine = StartCoroutine(ProcessLateBatch());
            }
        }

        void Start() => StartCoroutine(LoadSequence());

        private IEnumerator LoadSequence()
        {
            _snapshotTaken = true; 
            var ordered = _registered.OrderBy(o => o.Priority).ToList();
            int total = Mathf.Max(ordered.Count, 1);

            for (int i = 0; i < ordered.Count; i++)
            {
                var obj = ordered[i];
                OnStatusChanged?.Invoke($"Initializing {obj.GetType().Name}...");
                yield return StartCoroutine(SafeInit(obj));
                OnProgressChanged?.Invoke((float)(i + 1) / total * 0.8f);
                yield return null;
            }

            OnStatusChanged?.Invoke("Finalizing...");
            foreach (var obj in ordered)
            {
                try { obj.PostInitialize(); }
                catch (Exception e) { Debug.LogError($"PostInitialize failed on {obj.GetType().Name}: {e}"); }
                yield return null;
            }

            OnProgressChanged?.Invoke(1f);
            OnStatusChanged?.Invoke("Ready!");
            OnLoadingComplete?.Invoke();

        }

        private IEnumerator ProcessLateBatch()
        {
            while (true)
            {
                yield return null;

                if (_pendingLate.Count == 0)
                    break;

                var batch = _pendingLate.OrderBy(o => o.Priority).ToList();
                _pendingLate.Clear();
                int total = Mathf.Max(batch.Count, 1);

                for (int i = 0; i < batch.Count; i++)
                {
                    var obj = batch[i];
                    OnStatusChanged?.Invoke($"Initializing {obj.GetType().Name}...");
                    yield return StartCoroutine(SafeInit(obj));
                    OnProgressChanged?.Invoke((float)(i + 1) / total * 0.8f);
                    yield return null;
                }

                OnStatusChanged?.Invoke("Finalizing...");
                foreach (var obj in batch)
                {
                    try { obj.PostInitialize(); }
                    catch (Exception e) { Debug.LogError($"PostInitialize failed on {obj.GetType().Name}: {e}"); }
                    yield return null;
                }

                OnProgressChanged?.Invoke(1f);
                OnStatusChanged?.Invoke("Ready!");
                OnLoadingComplete?.Invoke();
            }

            _lateBatchRoutine = null;
        }

        private IEnumerator SafeInit(IInitializable obj)
        {
            var enumerator = obj.Initialize();
            float elapsed = 0f;

            while (true)
            {
                bool moved;
                try { moved = enumerator.MoveNext(); }
                catch (Exception e)
                {
                    Debug.LogError($"Init failed on {obj.GetType().Name}: {e}");
                    yield break;
                }

                if (!moved) break;

                elapsed += Time.deltaTime;
                if (elapsed > perObjectTimeoutSeconds)
                {
                    Debug.LogWarning($"{obj.GetType().Name} timed out during Initialize() after {perObjectTimeoutSeconds}s");
                    yield break;
                }

                yield return enumerator.Current;
            }
        }

        protected override void OnSingletonAwake()
        {
            base.OnSingletonAwake(); // still DontDestroyOnLoad
        }

        public override IEnumerator Initialize() { yield return null; }
        public override void PostInitialize() { }
    }
}