using System;
using System.Collections.Generic;
using System.Reflection;
using Script.Utility.Extend.DelegateTool;
using Utility.SingletonPatternSystem;

namespace Utility.EventSystem.Attribute
{
    /// <summary>
    /// 实现特性
    /// </summary>
    public class EventManager : EagerSingleton<EventManager>
    {
        private readonly Dictionary<string, List<Action>> _actionDictionary = new Dictionary<string, List<Action>>();

        private readonly Dictionary<string, List<Action<object[]>>> _parametricActionDictionary =
            new Dictionary<string, List<Action<object[]>>>();


        private void HandleEventOnRegister(object obj)
        {
            var classType = obj.GetType();
            var methods = classType.GetMethods();
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute(typeof(EventAttribute), false);
                if (attribute == null)
                {
                    continue;
                }

                var eventAttribute = (EventAttribute)attribute;
                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    var action = method.CreateDelegate(typeof(Action), obj) as Action;
                    if (!_actionDictionary.TryGetValue(eventAttribute.EventName, out var value))
                    {
                        _actionDictionary[eventAttribute.EventName] = new List<Action>();
                    }
                    else
                    {
                        value.Add(action);
                    }

                    var eventName = eventAttribute.EventName;
                    EventCenter.Instance.RegisterEvent(eventName, action);
                }
                else
                {
                    var action = DelegateTool.CreateDelegate(method, obj);
                    if (_parametricActionDictionary.TryGetValue(eventAttribute.EventName, out var value))
                    {
                        value.Add(action);
                    }
                    else
                    {
                        _parametricActionDictionary[eventAttribute.EventName] = new List<Action<object[]>> { action };
                    }
                }
            }
        }

        private void HandleEventUnRegister(object obj)
        {
            var classType = obj.GetType();
            var methods = classType.GetMethods();
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute(typeof(EventAttribute), false);
                if (attribute == null)
                {
                    continue;
                }

                var eventAttribute = (EventAttribute)attribute;
                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    var action = method.CreateDelegate(typeof(Action), obj) as Action;
                    if (!_actionDictionary.TryGetValue(eventAttribute.EventName, out var value))
                    {
                        _actionDictionary[eventAttribute.EventName] = new List<Action>();
                    }
                    else
                    {
                        value.Remove(action);
                    }

                    var eventName = eventAttribute.EventName;

                    EventCenter.Instance.UnregisterEvent(eventName, action);
                }
                else
                {
                    var action = DelegateTool.CreateDelegate(method, obj);
                    if (_parametricActionDictionary.TryGetValue(eventAttribute.EventName, out var value))
                    {
                        value.Remove(action);
                    }

                    EventCenter.Instance.UnregisterEvent(eventAttribute.EventName, action);
                }
            }
        }

        #region 处理签入的事件类

        public void RegisterEventHandler(object obj)
        {
            // eventHandlerList.Add(obj);
            HandleEventOnRegister(obj);
        }

        #endregion

        #region 处理签出的事件类

        public void UnregisterEventHandler(object obj)
        {
            // eventHandlerList.Remove(obj);
            HandleEventUnRegister(obj);
        }

        #endregion

        #region 生命周期

        ~EventManager()
        {
            EventCenter.Instance.UnregisterAllEvent();
        }

        #endregion
    }
}