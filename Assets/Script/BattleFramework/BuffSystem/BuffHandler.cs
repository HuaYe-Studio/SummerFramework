using System;
using System.Collections.Generic;
using BattleFramework.BuffSystem.BuffBase;
using Sirenix.OdinInspector;
using UnityEngine;
using Utility.EventSystem;

namespace BattleFramework.BuffSystem
{
    public class BuffHandler : MonoEventRegister, IBuffHandler
    {
        [ShowInInspector] public readonly LinkedList<BuffInfo> BuffInfos = new LinkedList<BuffInfo>();

        public override void Start()
        {
            base.Start();
        }

        public void AddBuff(int buffId, GameObject caster)
        {
          
        }

        public void RemoveBuff(int buffId, bool removeAll = true)
        {
            throw new NotImplementedException();
        }

        public void InterruptBuff(int buffId, bool interruptAll = true)
        {
            throw new NotImplementedException();
        }

        public void RegisterOnAddBuff(Action act)
        {
            throw new NotImplementedException();
        }

        public void UnregisterOnAddBuff(Action act)
        {
            throw new NotImplementedException();
        }

        public void RegisterOnRemoveBuff(Action act)
        {
            throw new NotImplementedException();
        }

        public void UnregisterOnRemoveBuff(Action act)
        {
            throw new NotImplementedException();
        }
    }
}