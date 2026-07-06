using System.Collections;
using System.Collections.Generic;
using Slafurry.Core.Interface;
using UnityEngine;
using UnityEngine.Audio;

namespace Slafurry.System.Audio
{
    public class SFXPlayer : MonoBehaviour, IInitializable
    {
        [SerializeField] private SFXData sfxData;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;

        private class CategoryPool
        {
            public AudioSource[] Sources;
            public int OldestIndex;
            public readonly Dictionary<AudioSource, string> Playing = new();
        }

        private readonly Dictionary<string, CategoryPool> _pools = new();

        public int Priority => 1;

        void Awake()
        {
            InitPools();
        }

        private void InitPools()
        {
            if (sfxData?.categories == null) return;

            foreach (var category in sfxData.categories)
            {
                var pool = new CategoryPool
                {
                    Sources = new AudioSource[category.poolSize]
                };

                for (int i = 0; i < category.poolSize; i++)
                {
                    GameObject go = new GameObject($"SFX_{category.categoryName}_{i}");
                    go.transform.SetParent(transform);

                    AudioSource source = go.AddComponent<AudioSource>();
                    source.playOnAwake = false;
                    source.outputAudioMixerGroup = sfxMixerGroup;

                    pool.Sources[i] = source;
                }

                _pools[category.categoryName] = pool;
            }
        }

        private AudioSource GetAvailableSource(CategoryPool pool)
        {
            foreach (var source in pool.Sources)
            {
                if (!source.isPlaying)
                {
                    pool.Playing.Remove(source);
                    source.loop = false;
                    return source;
                }
            }

            AudioSource stolen = pool.Sources[pool.OldestIndex];
            stolen.Stop();
            stolen.loop = false;
            pool.Playing.Remove(stolen);

            pool.OldestIndex = (pool.OldestIndex + 1) % pool.Sources.Length;

            return stolen;
        }

        private int CountPlaying(CategoryPool pool, string effectName)
        {
            int count = 0;
            foreach (var source in pool.Sources)
            {
                if (!source.isPlaying)
                {
                    pool.Playing.Remove(source);
                    continue;
                }

                if (pool.Playing.TryGetValue(source, out string currentName) && currentName == effectName)
                    count++;
            }
            return count;
        }

        // ========================= 3D =========================
        public void PlaySFX3D(string categoryName, string effectName, Vector3 pos, bool loop = false)
        {
            if (!TryPrepare(categoryName, effectName, out var pool, out var effect, out var clip))
                return;

            AudioSource source = GetAvailableSource(pool);
            source.transform.position = pos;
            source.spatialBlend = 1f;
            source.loop = loop;
            source.clip = clip;
            source.volume = effect.volume;
            pool.Playing[source] = effectName;
            source.Play();
        }

        // ========================= 2D =========================
        public void PlaySFX2D(string categoryName, string effectName, bool loop = false)
        {
            if (!TryPrepare(categoryName, effectName, out var pool, out var effect, out var clip))
                return;

            AudioSource source = GetAvailableSource(pool);
            source.transform.position = Vector3.zero;
            source.spatialBlend = 0f;
            source.loop = loop;
            source.clip = clip;
            source.volume = effect.volume;
            pool.Playing[source] = effectName;
            source.Play();
        }

        // ========================= STOP =========================
        public void StopAllSFX()
        {
            foreach (var pool in _pools.Values)
            {
                foreach (var source in pool.Sources)
                {
                    source.Stop();
                }

                pool.Playing.Clear();
            }
        }

        public void StopCategory(string categoryName)
        {
            if (!_pools.TryGetValue(categoryName, out var pool))
                return;

            foreach (var source in pool.Sources)
            {
                source.Stop();
            }

            pool.Playing.Clear();
        }

        public void StopSFX(string categoryName, string effectName)
        {
            if (!_pools.TryGetValue(categoryName, out var pool))
                return;

            foreach (var source in pool.Sources)
            {
                if (!source.isPlaying)
                    continue;

                if (pool.Playing.TryGetValue(source, out string currentName) &&
                    currentName == effectName)
                {
                    source.Stop();
                    pool.Playing.Remove(source);
                }
            }
        }

        // ========================= Shared lookup =========================
        private bool TryPrepare(string categoryName, string effectName, out CategoryPool pool, out SFXEffect effect, out AudioClip clip)
        {
            pool = null;
            effect = null;
            clip = null;

            if (!_pools.TryGetValue(categoryName, out pool))
            {
                Debug.LogWarning($"[SFXPlayer] Category '{categoryName}' tidak ditemukan.");
                return false;
            }

            effect = sfxData.GetSFXEffect(categoryName, effectName);
            if (effect == null || effect.clips == null || effect.clips.Length == 0)
            {
                Debug.LogWarning($"[SFXPlayer] SFX '{categoryName}/{effectName}' tidak ditemukan.");
                return false;
            }

            if (CountPlaying(pool, effectName) >= effect.maxSimultaneous)
                return false;

            clip = effect.clips[Random.Range(0, effect.clips.Length)];
            return true;
        }

        public IEnumerator Initialize()
        {
            throw new global::System.NotImplementedException();
        }

        public void PostInitialize()
        {
            throw new global::System.NotImplementedException();
        }
    }
}