using System;
using System.Collections.Generic;
using System.Linq;
using BattleFramework.BuffSystem.BuffBase;
using BattleFramework.BuffSystem.Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using Utility.EventSystem;
using Utility.LogSystem;

namespace BattleFramework.BuffSystem.BuffHandler
{
    public class BuffHandler : MonoEventRegister, IBuffHandler
    {
        [ShowInInspector] public readonly List<BuffInfo> BuffInfos = new();
        private Action _onAddBuff;
        private Action _onRemoveBuff;
        private bool _updated;
        private Action _forBuffStart;
        private Action _forBuffBeDestroy;

        public override void Start()
        {
            base.Start();
        }

        private void Update()
        {
            if (_updated) return;
            _updated = true;
            _forBuffBeDestroy?.Invoke();
            _forBuffStart?.Invoke();
            _forBuffBeDestroy = null;
            _forBuffStart = null;
        }

        private void LateUpdate()
        {
            _updated = false;
            var buffBeRemoved = false;
            for (var i = BuffInfos.Count - 1; i >= 0; i--)
            {
                var buff = BuffInfos[i];
                buff.OnUpdate();
                if (buff.IsEnable) continue;
                buff.OnRemove();
                buffBeRemoved = true;
                BuffInfos.Remove(buff);
                _forBuffBeDestroy += buff.OnBeDestroy;
            }

            if (buffBeRemoved) _onRemoveBuff?.Invoke();
        }


        #region Private Methods

        private void AddBuff(IBuffInfo buffInfo, GameObject caster)
        {
            if (!_updated) Update();
            var buff = (BuffInfo)buffInfo;
            if (buff.IsEmpty())
            {
                LogSystem.Instance.Log("尝试加入空Buff", LogLevelEnum.Debug);
                return;
            }

            // 无论是否可以添加都执行初始化和BuffAwake
            buff.Init(this, caster);
            buff.OnAwake();

            // 确定能添加Buff时
            _onAddBuff?.Invoke();
            // 检查是否已有同样的Buff
            var previous = BuffInfos.Find(_ => true);
            // 如果没有同样的Buff
            if (previous == null)
            {
                //Tag效果
                if (buff.BuffTag != BuffTag.BuffTag.None)
                {
                    //检查是否可以被已有Buff抵消
                    if (BuffInfos.Any(otherBuff =>
                            BuffManager.Instance.TagManager.CanAddWhenOtherBuffExist(buff.BuffTag, otherBuff.BuffTag)))
                    {
                        buff.SetIsEnable(false);
                        buff.OnBeDestroy();
                        return;
                    }

                    //检查buff是否可以抵消已有buff
                    for (var i = BuffInfos.Count - 1; i >= 0; i--)
                    {
                        if (BuffManager.Instance.TagManager.RemoveOtherBuffWhenTagAdd(buff.BuffTag,
                                BuffInfos[i].BuffTag))
                        {
                            RemoveBuff(BuffInfos[i]);
                        }
                    }

                    BuffInfos.Add(buff);
                    _forBuffStart += buff.OnStart;
                    return;
                }
            }

            // 如果有同样的Buff
            //一个Buff对象的Start不会重复执行
            //只有MultipleCount类型的Buff会重复添加   
            switch (previous.MultipleAddType)
            {
                case BuffMultipleAddType.ResetTime:
                    previous.ResetTimer();
                    break;
                case BuffMultipleAddType.MultipleLayer:
                    previous.ModifyLayer(1);
                    break;
                case BuffMultipleAddType.MultipleLayerAndResetTime:
                    previous.ModifyLayer(1);
                    previous.ResetTimer();
                    break;
                case BuffMultipleAddType.MultipleCount:
                    BuffInfos.Add(buff);
                    _forBuffStart += buff.OnStart;
                    break;
            }
        }

        /// <summary>
        /// 移除一个Buff，移除后执行OnBuffRemove
        /// </summary>
        /// <param name="buffInfo"></param>
        private void RemoveBuff(IBuffInfo buffInfo)
        {
            var buff = (BuffInfo)buffInfo;
            buff.SetIsEnable(false);
        }

        /// <summary>
        ///  移除一个Buff，移除后不执行OnBuffRemove
        /// </summary>
        /// <param name="buffInfo"></param>
        private void InterruptBuff(IBuffInfo buffInfo)
        {
            var buff = (BuffInfo)buffInfo;
            buff.SetIsEnable(false);
            BuffInfos.Remove(buff);
            _forBuffBeDestroy += buff.OnBeDestroy;
        }

        #endregion

        public void AddBuff(int buffId, GameObject caster)
        {
            var buff = BuffManager.Instance.GetBuff(buffId);
            AddBuff(buff, caster);
        }

        public void RemoveBuff(int buffId, bool removeAll = true)
        {
            var buff = BuffInfos.FirstOrDefault(buff => buff.ID == buffId);
            if (buff == null)
            {
                LogSystem.Instance.Log($"尝试从{gameObject.name}移除没有添加的Buff， id:{buffId}", LogLevelEnum.Debug);
                return;
            }

            if (buff.MultipleAddType == BuffMultipleAddType.MultipleCount && removeAll)
            {
                var buffs = BuffInfos.Where(b => b.ID == buffId);
                foreach (var bf in buffs)
                {
                    RemoveBuff(bf);
                }
            }
            else
            {
                RemoveBuff(buff);
            }
        }

        public void InterruptBuff(int buffId, bool interruptAll = true)
        {
            var buff = BuffInfos.FirstOrDefault(buff => buff.ID == buffId);
            if (buff == null)
            {
                LogSystem.Instance.Log($"尝试从{gameObject.name}打断没有添加的Buff， id:{buffId}", LogLevelEnum.Debug);
                return;
            }

            if (buff.MultipleAddType == BuffMultipleAddType.MultipleCount && interruptAll)
            {
                var buffs = BuffInfos.Where(b => b.ID == buffId);
                foreach (var bf in buffs)
                {
                    InterruptBuff(bf);
                }
            }
            else
            {
                InterruptBuff(buff);
            }
        }

        public void RegisterOnAddBuff(Action act)
        {
            _onAddBuff += act;
        }

        public void UnregisterOnAddBuff(Action act)
        {
            _onRemoveBuff -= act;
        }

        public void RegisterOnRemoveBuff(Action act)
        {
            _onRemoveBuff += act;
        }

        public void UnregisterOnRemoveBuff(Action act)
        {
            _onRemoveBuff -= act;
        }
    }
}