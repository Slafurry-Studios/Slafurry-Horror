using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Slafurry.Core.Abstract;
using Slafurry.Utils.Pooling;
using Slafurry.Utils.VFX;


namespace Slafurry.System.VFX
{

    public static class VFX
    {
        public static void Play(string key, Vector3 position)
        {
            VFXSystem.Instance.Play(key, position);
        }

        public static void Play(string key, Vector3 position, Quaternion rotation)
        {
            VFXSystem.Instance.Play(key, position, rotation);
        }

    }
    public class VFXSystem : GameSystem<VFXSystem>
    {
        [SerializeField] private VFXEntry[] vfxEntries;

        private readonly Dictionary<string, GenericPool<VFXCleaner>> _pools = new();
        public bool IsReady { get; private set; }

        public override IEnumerator Initialize()
        {
            foreach (var entry in vfxEntries)
                _pools[entry.key] = new GenericPool<VFXCleaner>(entry.prefab, transform, entry.defaultCapacity, entry.maxSize);
            yield return null;
        }

        public override void PostInitialize()
        {


        }
        protected override void OnSingletonAwake() => base.OnSingletonAwake();

        public VFXCleaner Play(string key, Vector3 position, Quaternion rotation = default)
        {
            Debug.Log($"[VFXSystem] Pools loaded: {_pools.Count}, looking for: '{key}'");
            if (!_pools.TryGetValue(key, out var pool))
            {
                Debug.LogWarning($"[VFXSystem] VFX '{key}' not found.");
                return null;
            }

            VFXCleaner instance = pool.Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetPool(pool);
            return instance;
        }
    }
}