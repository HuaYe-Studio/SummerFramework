
namespace Utility.SingletonPatternSystem
{
    public class Singleton<T> where T : class, new()
    {
        protected Singleton()
        {
        }

        private static T _inst = null;

        public static T Instance => _inst ??= new T();
        public static void Clear()
        {
            _inst = null;
        }
    }
}