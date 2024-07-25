using Utility.EventSystem;

namespace Utility.SingletonPatternSystem
{
    public class MonoSingletonEventRegister<T> : MonoEventRegister where T : MonoEventRegister
    {
        public static T Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this as T;
        }
    }
}