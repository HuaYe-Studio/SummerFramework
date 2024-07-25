using UnityEngine;
using Utility.EventSystem.Attribute;

namespace Utility.EventSystem
{
    public class MonoEventRegister : MonoBehaviour, IEventRegister
    {
        public void Register(object obj)
        {
            EventManager.Instance.RegisterEventHandler(obj);
        }

        public void Unregister(object obj)
        {
            EventManager.Instance.UnregisterEventHandler(obj);
        }

        public virtual void Start()
        {
            Register(this);
        }

        public void OnDestroy()
        {
            Unregister(this);
        }
    }
}