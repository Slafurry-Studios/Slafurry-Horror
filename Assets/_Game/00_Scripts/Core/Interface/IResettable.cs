namespace Slafurry.Core.Interface
{
    public interface IResettable
    {
        /// <summary>Clean state per session. 
        /// NEVER re-create resource that only used once (pool, subscribe event).</summary>
        void ResetState();
    }
}