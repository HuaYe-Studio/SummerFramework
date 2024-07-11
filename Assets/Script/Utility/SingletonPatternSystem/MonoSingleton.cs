using System;
using UnityEngine;

namespace Utility.SingletonPatternSystem
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        
        public static T Instance { get; private set; } = null;

        protected virtual void Awake()
        {
            Instance = (T)this;
        }
    }
}