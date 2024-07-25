using System;

namespace Utility.EventSystem.Attribute
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EventAttribute : System.Attribute
    {
        public EventAttribute(string eventName)
        {
            EventName = eventName;
        }

        public string EventName { get; }
    }
}