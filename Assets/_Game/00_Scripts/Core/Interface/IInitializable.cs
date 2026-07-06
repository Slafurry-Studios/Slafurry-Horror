using System.Collections;

namespace Slafurry.Core.Interface
{
    public interface IInitializable
    {
        /// <summary>Execution order from initialize phase. The smallest the higher the priority (run first).</summary>
        int Priority { get; }

        /// <summary>Internal setup, asset/data load. NEVER TOUCH OTHER OBJECTS HERE.</summary>
        IEnumerator Initialize();

        /// <summary>Object wiring. You can take other object references here.</summary>
        void PostInitialize();
    }
}
