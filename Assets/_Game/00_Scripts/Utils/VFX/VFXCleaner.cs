using UnityEngine;
using Slafurry.Utils.Pooling;

namespace Slafurry.Utils.VFX
{
    /// <summary>
    /// Attach this to the root of any Animator-driven VFX prefab
    /// (explosion, hit spark, muzzle flash, etc).
    ///
    /// Setup:
    /// 1. Add this component to the VFX prefab root.
    /// 2. On the Animator's last keyframe, add an Animation Event that
    ///    calls Clean() (no parameters needed).
    /// 3. Spawn the VFX through VFXSystem.Play(key, position) - it will
    ///    call SetPool() automatically so Clean() releases back to the
    ///    pool instead of destroying the GameObject.
    ///
    /// If this prefab is ever spawned WITHOUT going through VFXSystem
    /// (e.g. Instantiate() directly for a one-off case), Clean() falls
    /// back to Destroy(gameObject) - no pool reference required.
    /// </summary>
    public class VFXCleaner : MonoBehaviour, IPoolable
    {
        // Direct reference, not a delegate/Action - avoids a closure
        // allocation every time this VFX is spawned from the pool.
        private GenericPool<VFXCleaner> _ownerPool;

        /// <summary>Called once by the spawner (e.g. VFXSystem) right after Get().</summary>
        public void SetPool(GenericPool<VFXCleaner> pool)
        {
            _ownerPool = pool;
        }

        /// <summary>
        /// Call this from an Animation Event on the last frame of the VFX
        /// animation, or from a ParticleSystem "Stop Action" callback.
        /// </summary>
        public void Clean()
        {
            if (_ownerPool != null)
                _ownerPool.Release(this);
            else
                Destroy(gameObject);
        }

        // ===== IPoolable - called automatically by GenericPool =====
        public void OnSpawn()
        {
            // Restart the animation from frame 0 in case this instance
            // is being reused mid-animation from a previous play.
            if (TryGetComponent(out Animator animator))
                animator.Play(0, -1, 0f);
        }

        public void OnDespawn()
        {
            _ownerPool = null;
        }
    }
}