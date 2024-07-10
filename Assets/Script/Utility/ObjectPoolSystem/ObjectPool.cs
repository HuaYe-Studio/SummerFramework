using System.Collections;
using System;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using Utility.LogSystem;

namespace Utility.ObjectPoolSystem
{
    public class ObjectPool<T>
    {
        private readonly List<ObjectPoolContainer<T>> _list; //存储对象池中的对象容器
        private readonly Dictionary<T, ObjectPoolContainer<T>> _lookup; //存储对象池中的对象和对象容器的容器，键值对
        private readonly Func<T> _factoryFunc; //委托，用于创建对象
        private int _lastIndex = 0;

        public ObjectPool(Func<T> factoryFunc, int initialSize)
        {
            _factoryFunc = factoryFunc;
            _list = new List<ObjectPoolContainer<T>>(initialSize);
            _lookup = new Dictionary<T, ObjectPoolContainer<T>>(initialSize);
            Warm(initialSize);
        }

        private void Warm(int capacity)
        {
            for (var i = 0; i < capacity; i++) CreateContainer();
        }

        private ObjectPoolContainer<T> CreateContainer()
        {
            var container = new ObjectPoolContainer<T>();
            container.Item = _factoryFunc();
            _list.Add(container);
            return container;
        }

        public T GetItem()
        {
            ObjectPoolContainer<T> container = null;
            foreach (var t in _list)
            {
                _lastIndex++;
                if (_lastIndex > _list.Count - 1) _lastIndex = 0;

                if (_list[_lastIndex].Used)
                {
                    continue;
                }
                else
                {
                    container = _list[_lastIndex];
                    break;
                }
            }

            container ??= CreateContainer();

            container.Consume();
            _lookup.Add(container.Item, container);
            return container.Item;
        }

        public void ReleaseItem(object item)
        {
            ReleaseItem((T)item);
        }

        public void ReleaseItem(T item)
        {
            if (_lookup.ContainsKey(item))
            {
                var container = _lookup[item];
                container.Release();
                _lookup.Remove(item);
            }
            else
            {
                LogSystem.LogSystem.Instance.Log($"This object pool does not contain the item provided: {item}", LogLevelEnum.Debug);
                // Debug.Log("This object pool does not contain the item provided: " + item);
            }
        }

        public int Count => _list.Count;

        public int CountUsedItems => _lookup.Count;
    }
}