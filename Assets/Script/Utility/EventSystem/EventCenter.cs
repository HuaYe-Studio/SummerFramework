using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Utility.SingletonPatternSystem;

namespace Utility.EventSystem
{
    public class EventCenter : EagerSingleton<EventCenter>
    {
        [ShowInInspector]
        private readonly Dictionary<string, List<Action>> _eventDict = new Dictionary<string, List<Action>>(32);

        private readonly Dictionary<string, List<Action<object[]>>> _parametricEventDict =
            new Dictionary<string, List<Action<object[]>>>(32);

        private readonly List<string> _cacheList = new List<string>(16);

        #region 订阅事件

        public void RegisterEvent(string eventName, Action action)
        {
            if (!_eventDict.ContainsKey(eventName))
            {
                _eventDict[eventName] = new List<Action>();
            }

            _eventDict[eventName].Add(action);
        }

        public void RegisterEvent(string eventName, Action<object[]> action)
        {
            if (!_parametricEventDict.ContainsKey(eventName))
            {
                _parametricEventDict[eventName] = new List<Action<object[]>>();
            }

            _parametricEventDict[eventName].Add(action);
        }

        #endregion

        #region 取消订阅事件

        public void UnregisterEvent(string eventName, Action action)
        {
            if (_eventDict.TryGetValue(eventName, out var value))
            {
                value.Remove(action);
            }
        }

        public void UnregisterEvent(string eventName, Action<object[]> action)
        {
            if (!_parametricEventDict.TryGetValue(eventName, out var value)) return;
            value.RemoveAll(a => a == action);
        }

        public void UnregisterEvent(string eventName)
        {
            if (_eventDict.TryGetValue(eventName, out var value))
            {
                value.Clear();
            }
        }

        public void UnregisterAllEvent()
        {
            _eventDict.Clear();
        }

        #endregion

        #region 发布事件

        public void PublishEvent(string eventName, bool addToAche = false)
        {
            if (addToAche)
            {
                _cacheList.Add(eventName);
                return;
            }

            if (!_eventDict.TryGetValue(eventName, out var value)) return;
            foreach (var action in value)
            {
                action?.Invoke();
            }
        }

        public void PublishEvent(string eventName, object[] args, bool addToAche = false)
        {
            if (addToAche)
            {
                _cacheList.Add(eventName);
                return;
            }

            if (!_parametricEventDict.TryGetValue(eventName, out var value)) return;
            foreach (var action in value)
            {
                action?.Invoke(args);
            }
        }

        #endregion

        #region 延迟发布事件

        public void DelayPublishEvent(string eventName)
        {
            if (!_cacheList.Contains(eventName)) return;
            PublishEvent(eventName);
            _cacheList.Remove(eventName);
        }

        #endregion
    }
}