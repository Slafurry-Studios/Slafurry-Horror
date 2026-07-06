namespace Slafurry.Core.Abstract
{
    public abstract class GameSystem<T> : Singleton<T> where T : GameSystem<T>
    {
        protected override void OnSingletonAwake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
