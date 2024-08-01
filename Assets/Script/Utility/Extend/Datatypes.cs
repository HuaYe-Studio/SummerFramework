using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.Extend.Datatypes
{
    public abstract class Datatypes
    {
        [Serializable]
        public class SerializeList<T>
        {
            [SerializeField] public List<T> list = new();

            public List<T> ToList()
            {
                return list;
            }
          
        }

        [Serializable]
        public class SerializeDictionary<TKey, TValue> : ISerializationCallbackReceiver
        {
            public List<TKey> keys = new List<TKey>();
            public List<TValue> values = new List<TValue>();
            public Dictionary<TKey, TValue> Dictionary;

            public SerializeDictionary(Dictionary<TKey, TValue> dictionary)
            {
                Dictionary = dictionary;
             
            }

            public void OnBeforeSerialize()
            {
                keys.Clear();
                values.Clear();
                foreach (var kvp in Dictionary)
                {
                    keys.Add(kvp.Key);
                    values.Add(kvp.Value);
                }
            }

            public void OnAfterDeserialize()
            {
                Dictionary = new Dictionary<TKey, TValue>();
                for (var i = 0; i != Math.Min(keys.Count, values.Count); i++)
                {
                    Dictionary.Add(keys[i], values[i]);
                }
            }
        }

        [Obsolete("无效的类，使用UnityEvent代替")]
        [Serializable]
        public class SerializeAction
        {
            public Action Action;

            public SerializeAction(Action action)
            {
                Action = action;
            }

            public void Invoke()
            {
                Action?.Invoke();
            }
        }
    }
}