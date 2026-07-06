namespace Slafurry.Utils.Pooling
{
    /// <summary>
    /// Implement this on any Component that needs to reset its own state
    /// when reused from a pool. Optional - a poolable prefab doesn't have
    /// to implement this if it has no state to reset.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>Called every time this instance is taken out of the pool (Get).</summary>
        void OnSpawn();

        /// <summary>Called every time this instance is returned to the pool (Release).</summary>
        void OnDespawn();
    }
}