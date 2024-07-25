using Utility.EventSystem.Attribute;

namespace Utility.EventSystem
{
    public class EventRegister: IEventRegister
    {
        protected EventRegister()
        {
        }

        public void Register(object obj)
        {
            EventManager.Instance.RegisterEventHandler(obj);
        }

        public void Unregister(object obj)
        {
            EventManager.Instance.UnregisterEventHandler(obj);
        }
    }
  
}