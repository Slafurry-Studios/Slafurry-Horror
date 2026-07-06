namespace Slafurry.Core.Abstract
{
    public abstract class LocalSingleton<T> : Singleton<T> where T : LocalSingleton<T>
    {
        protected override void OnSingletonAwake()
        {
            // Emptied, so this singleton won't persist across scenes
        }
    }
}
