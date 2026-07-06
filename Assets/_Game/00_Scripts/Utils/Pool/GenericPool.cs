using UnityEngine;
using UnityEngine.Pool;

namespace Slafurry.Utils.Pooling
{
    /// <summary>
    /// Reusable generic pool for any prefab with a Component of type T.
    /// This is a plain C# class (NOT a MonoBehaviour) - instantiate it
    /// inside whatever Manager/System owns the pooled objects
    /// (e.g. EnemyManager owns a GenericPool&lt;EnemyController&gt;).
    ///
    /// If T implements IPoolable, OnSpawn()/OnDespawn() fire automatically
    /// on Get()/Release() - no need to call them manually.
    /// </summary>
    public class GenericPool<T> where T : Component
    {
        private readonly ObjectPool<T> _pool;
        private readonly T _prefab;
        private readonly Transform _parent;

        public int CountActive => _pool.CountActive;
        public int CountInactive => _pool.CountInactive;

        public GenericPool(T prefab, Transform parent = null, int defaultCapacity = 10, int maxSize = 100, bool collectionCheck = true)
        {
            _prefab = prefab;
            _parent = parent;

            _pool = new ObjectPool<T>(
                createFunc: CreateItem,
                actionOnGet: OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: OnDestroyItem,
                collectionCheck: collectionCheck,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        public T Get() => _pool.Get();

        public void Release(T item) => _pool.Release(item);

        /// <summary>Pre-warm the pool by creating N inactive instances up front, avoiding runtime spikes.</summary>
        public void Prewarm(int count)
        {
            var temp = new T[count];
            for (int i = 0; i < count; i++)
                temp[i] = Get();

            for (int i = 0; i < count; i++)
                Release(temp[i]);
        }

        public void Clear() => _pool.Clear();

        private T CreateItem()
        {
            var item = Object.Instantiate(_prefab, _parent);
            return item;
        }

        private void OnGet(T item)
        {
            item.gameObject.SetActive(true);
            if (item is IPoolable poolable) poolable.OnSpawn();
        }

        private void OnRelease(T item)
        {
            if (item is IPoolable poolable) poolable.OnDespawn();
            item.gameObject.SetActive(false);
        }

        private void OnDestroyItem(T item)
        {
            Object.Destroy(item.gameObject);
        }
    }
}
